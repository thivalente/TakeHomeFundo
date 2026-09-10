namespace FundoTakeHome.Backend.Features.SubmitApplication.Domain.Enums;

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
