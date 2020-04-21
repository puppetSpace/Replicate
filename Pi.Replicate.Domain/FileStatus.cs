namespace Pi.Replicate.Domain
{
    //todo check if statuses are set correctly
    public enum FileStatus
    {
        New = 0,
        Changed = 1,
        Processed = 2,
        Received = 3,
        Handled = 5
    }
}