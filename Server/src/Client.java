import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.net.Socket;

public class Client
{
    public DataOutputStream output;
    public DataInputStream input;
    public Socket socket;

    public String name;


    public Client(DataOutputStream output, DataInputStream input, Socket socket, String name)
    {
        this.output = output;
        this.input = input;
        this.socket = socket;
        this.name = name;
    }
}
