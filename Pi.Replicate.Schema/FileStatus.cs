namespace Pi.Replicate.Schema
{
    //todo check if statuses are set correctly
    public enum FileStatus
    {
        New = 0,
        Sent = 1,
        ReceivedIncomplete = 2,
        ReceivedComplete = 3,
        UploadSucessful = 4
    }
}