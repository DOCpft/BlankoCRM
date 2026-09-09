using BlankoCRM.Application.DTOs;
using FluentValidation;

namespace BlankoCRM.Application.Validation;

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(request => request.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(request => request.Name).MaximumLength(100);
        RuleFor(request => request.OrganizationName).MaximumLength(100);
    }
}

public sealed class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.Password).NotEmpty();
    }
}

public sealed class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Phone).NotEmpty().MaximumLength(20);
        RuleFor(request => request.Telegram).MaximumLength(100);
        RuleFor(request => request.Notes).MaximumLength(500);
    }
}

public sealed class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(request => request.Name).MaximumLength(200);
        RuleFor(request => request.Phone).MaximumLength(20);
        RuleFor(request => request.Telegram).MaximumLength(100);
        RuleFor(request => request.Notes).MaximumLength(500);
    }
}

public sealed class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
{
    public CreateServiceRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.DurationMinutes).InclusiveBetween(1, 1440);
        RuleFor(request => request.Price).GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999.99m);
        RuleFor(request => request.Description).MaximumLength(500);
    }
}

public sealed class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
{
    public UpdateServiceRequestValidator()
    {
        RuleFor(request => request.Name).MaximumLength(100);
        RuleFor(request => request.DurationMinutes).InclusiveBetween(1, 1440).When(request => request.DurationMinutes.HasValue);
        RuleFor(request => request.Price).GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999.99m).When(request => request.Price.HasValue);
        RuleFor(request => request.Description).MaximumLength(500);
    }
}

public sealed class ServiceInputValidator : AbstractValidator<ServiceInput>
{
    public ServiceInputValidator()
    {
        RuleFor(input => input.ServiceId).NotEmpty();
        RuleFor(input => input.DurationMinutes).InclusiveBetween(1, 1440).When(input => input.DurationMinutes.HasValue);
        RuleFor(input => input.Price).GreaterThanOrEqualTo(0).When(input => input.Price.HasValue);
        RuleFor(input => input.Quantity).GreaterThan(0);
    }
}

public sealed class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator(IValidator<ServiceInput> serviceInputValidator)
    {
        RuleFor(request => request.ClientId).NotEmpty();
        RuleFor(request => request.Services).NotEmpty().SetValidator(new ServiceInputCollectionValidator(serviceInputValidator));
        RuleFor(request => request.StartTime).NotEmpty();
        RuleFor(request => request.Notes).MaximumLength(500);
    }
}

public sealed class ServiceInputCollectionValidator : AbstractValidator<List<ServiceInput>>
{
    public ServiceInputCollectionValidator(IValidator<ServiceInput> serviceInputValidator)
    {
        RuleForEach(inputs => inputs).SetValidator(serviceInputValidator);
    }
}

public sealed class UpdateAppointmentRequestValidator : AbstractValidator<UpdateAppointmentRequest>
{
    public UpdateAppointmentRequestValidator()
    {
        RuleFor(request => request.Notes).MaximumLength(500);
    }
}

public sealed class CreateSlotRequestValidator : AbstractValidator<CreateSlotRequest>
{
    public CreateSlotRequestValidator()
    {
        RuleFor(request => request.slotTime).NotEmpty();
    }
}

public sealed class UpdateSlotRequestValidator : AbstractValidator<UpdateSlotRequest>
{
    public UpdateSlotRequestValidator()
    {
        RuleFor(request => request.slotTime).NotEmpty().When(request => request.slotTime.HasValue);
    }
}

public sealed class CreateManySlotsRequestValidator : AbstractValidator<CreateManySlotsRequest>
{
    public CreateManySlotsRequestValidator()
    {
        RuleFor(request => request.Slots).NotEmpty();
        RuleForEach(request => request.Slots)
            .Must(slot => slot.slotTime != default)
            .WithMessage("Slot time is required.");
    }
}

public sealed class CreateSlotRequestListValidator : AbstractValidator<List<CreateSlotRequest>>
{
    public CreateSlotRequestListValidator(IValidator<CreateSlotRequest> slotValidator)
    {
        RuleFor(requests => requests).NotEmpty();
        RuleForEach(requests => requests).SetValidator(slotValidator);
    }
}
