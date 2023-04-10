import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

public class Server
{
    //the server socket (for accepting new connections)
    private ServerSocket serverSocket;
    //Roomcodes and Room objects are mapped here
    private HashMap<String, Room> rooms = new HashMap<>();
    private List<Room> publicRooms = new ArrayList<>();

    //Server reference
    private final Server server = this;

    //A Formatter for date and time log outputs
    public void deleteRoom(String code)
    {
        Room r = rooms.get(code);
        r.closeRoom();
        rooms.remove(code);
        publicRooms.remove(r);

        //LOG
        System.out.println("Room " + code + " successfully deleted");
    }

    /**
     * Starts the Server
     */
    public void start()
    {

        try
        {
            //create the listening server socket on port 8501
            serverSocket = new ServerSocket(8051);

            //LOG
            System.out.println("Server started! IP 185.245.96.48 PORT 8051");

            //a endless loop for accepting new connections
            while (true)
            {
                //accept a connection from a client
                Socket tempClient = serverSocket.accept();

                //LOG
                System.out.println("Client connected!");

                //create a new thread for the connected client to free up main thread resources for new connections
                Thread t = null;
                t = new Thread(new Runnable()
                {
                    @Override
                    public void run()
                    {
                        //cache the connected client in the thread (if not it could be overwritten by a new connection)
                        Socket client = tempClient;

                        try
                        {
                            //get the input stream of the client (TCP)
                            DataInputStream input = new DataInputStream(client.getInputStream());
                            //get the output stream of the client (TCP)
                            DataOutputStream output = new DataOutputStream(client.getOutputStream());
                            //A buffer for incoming data
                            byte[] readData = new byte[10];

                            //read incoming data into the buffer
                            /**
                             * INPUT FORMAT: [action, free space]
                             */
                            input.read(readData);

                            //check for the incoming actions
                            switch (readData[0])
                            {
                                /**
                                 * !!!!!!!!!!!!!!!!!!!      0 is not used because of possible empty arrays read in because of inital connection errors  !!!!!!!!!!!!!!!!!!!!
                                 */
                                case 0:
                                case 126:
                                    client.close();
                                    client = null;
                                    Integer.parseInt("crash");
                                    break;
                                case 3:
                                    byte[] roomCodeToClose = new byte[10];
                                    input.read(roomCodeToClose);
                                    String roomCodeToCloseString = new String(roomCodeToClose, StandardCharsets.US_ASCII);
                                    deleteRoom(roomCodeToCloseString);
                                    break;

                                //this case generates a new room and sends the room code to the requester (not for join)
                                case 2:
                                    //cache the generated room code
                                    byte[] code = generateRoomCode();
                                    //send the room code to the requester
                                    output.write(code);

                                    //create the room object
                                    Room room = new Room(new String(code, StandardCharsets.US_ASCII), server);
                                    //map the room object to the room code
                                    rooms.put(new String(code, StandardCharsets.US_ASCII), room);

                                    //LOG
                                    System.out.println("Room " + new String(code, StandardCharsets.US_ASCII) + " successfully created!");
                                    client.close();
                                    client = null;
                                    break;
                                //in this case a player wants to join an existing room (even the room creator will join via this method)
                                case 1:

                                    //fill the buffer with the incoming data (second data send after inital data transfer, no server response except TCP..)
                                    /**
                                     * INPUT FORMAT: [roomcode]
                                     */
                                    input.read(readData);

                                    //deocde the roomcode to a string (ascii -> string)
                                    String roomCode = new String(readData, StandardCharsets.US_ASCII);

                                    //read the third message
                                    /**
                                     * INPUT FORMAT: [name]
                                     */
                                    input.read(readData);

                                    //deocde the players name
                                    String name = new String(readData, StandardCharsets.US_ASCII);
                                    name = name.replace("?", " ");

                                    input.read(readData);

                                    int characterID = readData[0];

                                    //check if the room exists
                                    if (rooms.containsKey(roomCode))
                                    {
                                        //create the reference to the client
                                        Client client1 = new Client(output, input, client, name, characterID);

                                        //move the client to the room
                                        rooms.get(roomCode).addClient(client1);
                                    } else
                                    {
                                        //room is not available, so the player should automatically go back to the start menu (kick action)
                                        output.write(new byte[]{127, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                                    }

                                    break;
                                    //For Public Match
                                case 4:
                                    boolean success = false;

                                    for(int i = 0; i < publicRooms.size(); i++)
                                    {
                                        Room temp = publicRooms.get(i);
                                        if(temp.playerCount < temp.players.length)
                                        {
                                            String sendRoomCode = temp.roomCode;
                                            output.write(sendRoomCode.getBytes(StandardCharsets.US_ASCII));
                                            success = true;
                                            client.close();
                                            client = null;
                                            break;
                                        }
                                    }

                                    if(!success)
                                    {
                                        byte[] tempRoomCode = generateRoomCode();
                                        Room tempRoom = new Room(new String(tempRoomCode, StandardCharsets.US_ASCII), server);
                                        publicRooms.add(tempRoom);
                                        rooms.put(new String(tempRoomCode, StandardCharsets.US_ASCII), tempRoom);
                                        output.write(tempRoomCode);

                                        System.out.println("Room " + new String(tempRoomCode, StandardCharsets.US_ASCII) + " successfully created!");
                                        client.close();
                                        client = null;
                                        break;
                                    }
                            }

                            System.out.println("Init Thread finished");

                        } catch (Exception e)
                        {
                            System.out.println("Client disconnected!");
                        }
                    }
                });
                t.start();
            }
        } catch (Exception e)
        {
            //do not end the program on any main thread errors (unwated connection losses), because other players wont be able to connect to the server
        }
    }

    /**
     * Generates a ascii byte[] room code
     * please see the ascii table
     *
     * @return
     */
    private byte[] generateRoomCode()
    {
        //buffer to store the code
        byte[] code = new byte[10];

        for (int i = 0; i < code.length; i++)
        {
            //for the first 5 indexes user numbers (1-9)
            if (i < code.length / 2)
            {
                code[i] = (byte) (Math.random() * 8 + 49);
            }
            //for the last 5 indexes use letters (A-Z)
            else
            {
                code[i] = (byte) (Math.random() * 25 + 97);
                while(code[i] == 111)
                {
                    code[i] = (byte) (Math.random() * 25 + 97);
                }
            }
        }

        //decode the generated room code to a string
        String roomCode = new String(code, StandardCharsets.US_ASCII);

        //in case the room code does allready exists, the roomcode generation will be called recursively
        if (rooms.containsKey(roomCode))
        {
            code = generateRoomCode();
        }

        return code;
    }
}
