namespace Pi.Replicate.Domain
{
    //todo check if statuses are set correctly
    public enum FileStatus
    {
        MetadataSent = 0,
        EofSent = 1,
        Processed = 2,
        Received = 3,
        Handled = 4
    }
}