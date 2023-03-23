import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.net.Socket;

/**
 * Instance of a Client connected
 */
public class Client
{
    public DataOutputStream output;
    public DataInputStream input;
    public Socket socket;

    public String name;

    public long lastTimeSendTimeOutCheck = -1;


    public Client(DataOutputStream output, DataInputStream input, Socket socket, String name)
    {
        this.output = output;
        this.input = input;
        this.socket = socket;
        this.name = name;
    }
}
