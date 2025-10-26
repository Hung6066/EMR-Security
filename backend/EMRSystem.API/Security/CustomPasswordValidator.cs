public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
{
    private readonly IPasswordPolicyService _policy;
    public CustomPasswordValidator(IPasswordPolicyService policy) { _policy = policy; }

    public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
    {
        var errors = await _policy.ValidateAsync(user, password);
        return errors.Count == 0 ? IdentityResult.Success
                                 : IdentityResult.Failed(errors.Select(e => new IdentityError { Description = e }).ToArray());
    }
}