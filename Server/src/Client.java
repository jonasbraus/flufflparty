import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.net.Socket;

public class Client
{
    public DataOutputStream output;
    public DataInputStream input;
    public Socket socket;


    public Client(DataOutputStream output, DataInputStream input, Socket socket)
    {
        this.output = output;
        this.input = input;
        this.socket = socket;
    }
}
