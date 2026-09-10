namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.Enums;

public enum ApplicationStatusEnum
{
    Approved
}

public enum EntityOperationEnum
{
    Created,
    Updated
}

public enum OutboxEventTypeEnum
{
    ApplicationApproved
}
