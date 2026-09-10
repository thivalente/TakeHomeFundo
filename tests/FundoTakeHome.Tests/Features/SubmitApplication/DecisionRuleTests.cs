using ErrorOr;
using FundoTakeHome.Backend.Features.SubmitApplication.Application;
using FundoTakeHome.Backend.Features.SubmitApplication.Application.DecisionRules;
using FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Common.Errors;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Entities;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects;
using Moq;
using Shouldly;

namespace FundoTakeHome.Tests.Features.SubmitApplication;

public sealed class DecisionRuleTests
{
    [Fact]
    public async Task ShouldReturnNoErrors_WhenNoDecisionRuleDenies()
    {
        var rule = new Mock<IDecisionRule<DecisionRuleInput>>();
        rule.Setup(decisionRule => decisionRule.EvaluateAsync(It.IsAny<DecisionRuleInput>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

        var result = await new DecisionRuleEngine([rule.Object]).EvaluateAsync(CreateInput(), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnAllDenials_WhenMultipleRulesMatch()
    {
        var stateRule = new Mock<IDecisionRule<DecisionRuleInput>>();
        stateRule.Setup(rule => rule.EvaluateAsync(It.IsAny<DecisionRuleInput>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<ErrorOr<Success>>(ApplicationDenialErrors.StateIsNewYork));

        var ssnRule = new Mock<IDecisionRule<DecisionRuleInput>>();
        ssnRule.Setup(rule => rule.EvaluateAsync(It.IsAny<DecisionRuleInput>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<ErrorOr<Success>>(ApplicationDenialErrors.SsnBlacklisted));

        var result = await new DecisionRuleEngine([stateRule.Object, ssnRule.Object]).EvaluateAsync(CreateInput(), CancellationToken.None);

        result.Count.ShouldBe(2);
        result.ShouldContain(error => error.Code == ApplicationDenialErrors.StateIsNewYork.Code);
        result.ShouldContain(error => error.Code == ApplicationDenialErrors.SsnBlacklisted.Code);
        stateRule.Verify(rule => rule.EvaluateAsync(It.IsAny<DecisionRuleInput>(), CancellationToken.None), Times.Once);
        ssnRule.Verify(rule => rule.EvaluateAsync(It.IsAny<DecisionRuleInput>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnStateDenial_WhenApplicationIsFromNewYork()
    {
        var result = await new StateIsNyRule().EvaluateAsync(CreateInput(state: "NY"), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Code == ApplicationDenialErrors.StateIsNewYork.Code);
    }

    [Fact]
    public async Task ShouldReturnSsnDenial_WhenSsnIsBlacklisted()
    {
        var reader = new Mock<IBlacklistSsnReader>();
        reader.Setup(blacklist => blacklist.ExistsAsync(It.IsAny<Ssn>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await new BlacklistedSsnRule(reader.Object).EvaluateAsync(CreateInput(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Code == ApplicationDenialErrors.SsnBlacklisted.Code);
    }

    private static DecisionRuleInput CreateInput(string state = "CA", string ssn = "123456789") =>
        new(Customer.Create(ssn, "Jane", "Doe", "1 Main Street", state, "Fundo").Value!);
}
