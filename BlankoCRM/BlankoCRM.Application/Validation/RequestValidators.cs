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
        RuleFor(input => input.Quantity)
            .GreaterThan(0)
            .When(input => input.Quantity.HasValue);
    }

public sealed class CreateOccupiedSlotAppointmentDataValidator
    : AbstractValidator<CreateOccupiedSlotAppointmentData>
{
    public CreateOccupiedSlotAppointmentDataValidator(IValidator<ServiceInput> serviceInputValidator)
    {
        RuleFor(request => request.ClientId).NotEmpty();
        RuleFor(request => request.Services)
            .NotEmpty()
            .SetValidator(new ServiceInputCollectionValidator(serviceInputValidator));
        RuleFor(request => request.Notes).MaximumLength(500);
    }
}

public sealed class CreateOccupiedSlotRequestValidator : AbstractValidator<CreateOccupiedSlotRequest>
{
    public CreateOccupiedSlotRequestValidator(
        IValidator<CreateOccupiedSlotAppointmentData> appointmentDataValidator)
    {
        RuleFor(request => request.StartTime).NotEmpty();
        RuleFor(request => request.EndTime)
            .GreaterThan(request => request.StartTime)
            .When(request => request.EndTime.HasValue)
            .WithMessage("Время окончания должно быть позже времени начала.");
        RuleFor(request => request.AppointmentData)
            .NotNull()
            .SetValidator(appointmentDataValidator);
    }
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

public sealed class CreateFreeSlotRequestValidator : AbstractValidator<CreateFreeSlotRequest>
{
    public CreateFreeSlotRequestValidator()
    {
        RuleFor(request => request.StartTime).NotEmpty();
        RuleFor(request => request.EndTime)
            .GreaterThan(request => request.StartTime)
            .WithMessage("Время окончания должно быть позже времени начала.");
    }
}

public sealed class UpdateSlotRequestValidator : AbstractValidator<UpdateSlotRequest>
{
    public UpdateSlotRequestValidator()
    {
        RuleFor(request => request.EndTime)
            .GreaterThan(request => request.StartTime)
            .When(request => request.StartTime.HasValue && request.EndTime.HasValue)
            .WithMessage("Время окончания должно быть позже времени начала.");
    }
}

public sealed class CreateManySlotsRequestValidator : AbstractValidator<CreateManySlotsRequest>
{
    public CreateManySlotsRequestValidator()
    {
        RuleFor(request => request.Slots).NotEmpty();
        RuleForEach(request => request.Slots)
            .SetValidator(new CreateFreeSlotRequestValidator());
    }
}

public sealed class CreateFreeSlotRequestListValidator : AbstractValidator<List<CreateFreeSlotRequest>>
{
    public CreateFreeSlotRequestListValidator(IValidator<CreateFreeSlotRequest> slotValidator)
    {
        RuleFor(requests => requests).NotEmpty();
        RuleForEach(requests => requests).SetValidator(slotValidator);
    }
}

public sealed class VkMiniAppAuthRequestValidator : AbstractValidator<VkMiniAppAuthRequest>
{
    public VkMiniAppAuthRequestValidator()
    {
        RuleFor(x => x.VkParams)
            .NotEmpty()
            .WithMessage("Параметры VK обязательны.");

        RuleFor(x => x.VkParams)
            .Must(p => p.ContainsKey("sign") && !string.IsNullOrWhiteSpace(p["sign"]))
            .WithMessage("Отсутствует подпись VK (sign).")
            .When(x => x.VkParams is not null && x.VkParams.Count > 0);

        RuleFor(x => x.VkParams)
            .Must(p => p.ContainsKey("vk_user_id") && long.TryParse(p["vk_user_id"], out _))
            .WithMessage("Отсутствует или некорректен vk_user_id.")
            .When(x => x.VkParams is not null && x.VkParams.Count > 0);
    }
}
