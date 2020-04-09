namespace Pi.Replicate.Domain
{
    //todo check if statuses are set correctly
    public enum FileStatus
    {
        New = 0,
        Changed = 1,
        Processed = 2,
        ReceivedIncomplete = 3,
        ReceivedComplete = 4,
        UploadSucessful = 5
    }
}