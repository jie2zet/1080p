namespace CMP.Services.Rfid;

public interface IRfidReaderService
{
    bool IsStarted { get; }

    void Start();

    void Stop();
}
