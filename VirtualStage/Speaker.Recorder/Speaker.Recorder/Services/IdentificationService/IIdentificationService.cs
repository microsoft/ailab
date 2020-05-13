namespace Speaker.Recorder.Services.IdentificationService
{
    public interface IIdentificationService
    {
        string GetRawIdentifier();

        // As per Azure Documentation, e.g to create a blob, we need:
        // - 3 to 63 characters
        // - Only lowercase, numbers and hyphens
        // - Begin and end with a letter or a number
        // - NO consecutive hyphens
        string GetSanitizedIdentifier();
    }
}
