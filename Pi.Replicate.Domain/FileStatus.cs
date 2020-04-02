namespace Pi.Replicate.Domain
{
    //todo check if statuses are set correctly
    public enum FileStatus
    {
        New = 0,
        Processed = 1,
        ReceivedIncomplete = 2,
        ReceivedComplete = 3,
        UploadSucessful = 4
    }
}