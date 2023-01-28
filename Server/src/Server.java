import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.HashMap;

public class Server
{
    private ServerSocket serverSocket;
    private HashMap<String, Room> rooms = new HashMap<>();

    private final Server server = this;

    private static DateTimeFormatter dtf = DateTimeFormatter.ofPattern("yyyy/MM/dd HH:mm:ss");

    public static String getDateTime()
    {
        return dtf.format(LocalDateTime.now());
    }

    public void deleteRoom(String code)
    {
        rooms.get(code).closeRoom();
        rooms.remove(code);


        System.out.println("[" + getDateTime() + "] Room" + code + " successfully deleted");
    }

    public void start()
    {

        try
        {
            serverSocket = new ServerSocket(8051);

            System.out.println("[" + getDateTime() + "] Server started! IP 185.245.96.48 PORT 8051");

            while(true)
            {
                Socket tempClient = serverSocket.accept();

                System.out.println("[" + getDateTime() + "] Client with IP " + tempClient.getInetAddress().getHostName() + " connected!");

                new Thread(new Runnable()
                {
                    @Override
                    public void run()
                    {
                        Socket client = tempClient;

                        try
                        {
                            DataInputStream input = new DataInputStream(client.getInputStream());
                            DataOutputStream output = new DataOutputStream(client.getOutputStream());
                            byte[] readData = new byte[10];

                            input.read(readData);

                            switch(readData[0])
                            {
                                case 0:
                                    byte[] code = generateRoomCode();
                                    output.write(code);

                                    Room room = new Room(new String(code, StandardCharsets.US_ASCII), server);
                                    rooms.put(new String(code, StandardCharsets.US_ASCII), room);

                                    System.out.println("[" + getDateTime() + "] Room " + new String(code, StandardCharsets.US_ASCII) + " successfully created!");
                                    break;
                                case 1:

                                    input.read(readData);

                                    String roomCode = new String(readData, StandardCharsets.US_ASCII);

                                    if(rooms.containsKey(roomCode))
                                    {
                                        Client client1 = new Client(output, input, client);

                                        rooms.get(roomCode).addClient(client1);
                                    }
                                    else
                                    {
                                        output.write(new byte[]{127, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                                    }

                                    break;
                            }

                        } catch (Exception e){

                        }
                    }
                }).start();
            }
        }
        catch(Exception e){e.printStackTrace();}
    }

    private byte[] generateRoomCode()
    {
        byte[] code = new byte[10];

        for(int i = 0; i < code.length; i++)
        {
            if(i < code.length / 2)
            {
                code[i] = (byte)(Math.random() * 8 + 49);
            }
            else
            {
                code[i] = (byte)(Math.random() * 25 + 97);
            }
        }

        String roomCode = new String(code, StandardCharsets.US_ASCII);

        //avoide double codes (just in case^^)
        if(rooms.containsKey(roomCode))
        {
            code = generateRoomCode();
        }

        return code;
    }
}
