namespace Speaker.Recorder.Services.IdentificationService
{
    public class CustomIdentificationService : BaseIdentificationService
    {
        public string SessionsIdentifier { get; }

        public CustomIdentificationService(string customIdentification)
        {
            this.SessionsIdentifier = customIdentification;
        }

        public override string GetRawIdentifier()
        {
            return this.SessionsIdentifier;
        }
    }
}
