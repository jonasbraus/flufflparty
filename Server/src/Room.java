import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.net.ServerSocket;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

/**
 * this class handels all operations / syncs performed in a single room
 */
public class Room
{
    //tells how many clients can connect to a room
    private Client[] players = new Client[2];
    //the array of players start positions
    private Vector3[] startPositions;
    //the generated roomcode
    private String roomCode;
    //the instance to the main server
    private Server server;

    //all threads created stored in this list to close them after room closure
    private List<Thread> threads = new ArrayList<>();

    //count of players in the game
    private int playerCount = 0;

    //tells players turn (array index)
    private int currentPlayer = 0;

    private long timeOut = 0;
    private Timer t;

    public Room(String roomCode, Server server)
    {
        timeOut = System.currentTimeMillis();

        this.roomCode = roomCode;
        startPositions = new Vector3[4];

        //set up the start positions
        startPositions[0] = new Vector3(96, 1, 126);
        startPositions[1] = new Vector3(92, 1, 126);
        startPositions[2] = new Vector3(96, 1, 128);
        startPositions[3] = new Vector3(92, 1, 128);

        this.server = server;

        //calculates a room timeout
        try
        {
            t = new Timer();
            TimerTask tt = new TimerTask()
            {
                @Override
                public void run()
                {
                    try
                    {
                        for (int p = 0; p < players.length; p++)
                        {
                            if (players[p] != null)
                            {
                                if (System.currentTimeMillis() - players[p].lastTimeSendTimeOutCheck > 30000 && players[p].lastTimeSendTimeOutCheck != -1)
                                {
                                    server.deleteRoom(roomCode);
                                }

                                players[p].output.write(new byte[]{126, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                            }
                        }
                    } catch (Exception e)
                    {

                    }

                    if (System.currentTimeMillis() - timeOut > 600000)
                    {
                        System.out.println("[" + Server.getDateTime() + "] Room " + roomCode + " closed due to timeout!");
                        server.deleteRoom(roomCode);
                    }
                }
            };

            t.scheduleAtFixedRate(tt, 0, 10000);
        } catch (Exception e)
        {
        }
    }

    /**
     * Adds a Client to the game (on new player joined the room)
     * @param client
     */
    public void addClient(Client client)
    {
        //the timeout variable for a single player
        timeOut = System.currentTimeMillis();

        //loops through all players existing and sends the needed information
        for (int i = 0; i < players.length; i++)
        {
            //places the new joined player in the next free slot
            if (players[i] == null)
            {
                players[i] = client;
                playerCount++;

                //LOG
                System.out.println("[" + Server.getDateTime() + "] " + client.name.replace(" ", "") + " joined room " + roomCode + " successfully as player " + i);

                //creates a thread for the connection with the joined player
                Thread t = new Thread(new Runnable()
                {
                    @Override
                    public void run()
                    {
                        //assignes a player id
                        int player = playerCount - 1;

                        DataInputStream input = null;
                        try
                        {
                            //tries to get the input stream (TCP)
                            input = new DataInputStream(players[player].socket.getInputStream());
                        } catch (IOException e)
                        {
                            throw new RuntimeException(e);
                        }

                        while (true)
                        {
                            try
                            {
                                //this is a buffer for all data being received by the player
                                byte[] readData = new byte[10];

                                //read the data to the buffer
                                input.read(readData);

                                //reset the timeout once something is received
                                timeOut = System.currentTimeMillis();


                                /*
                                ----------------------------------------
                                ----------------------------------------
                                ----------------------------------------
                                HERE IS THE ROOM / GAME LOGIC
                                ----------------------------------------
                                ----------------------------------------
                                ----------------------------------------
                                 */
                                //Checks which action (array[0]) is received
                                switch (readData[0])
                                {

                                    //spieler hat gewürfelt
                                    //1 x, x = zahl
                                    case 3:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //sync to all OTHER players
                                            if (i != player)
                                            {
                                                players[i].output.write(new byte[]{3, (byte) player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;
                                    //pfeil wurde ausgewählt
                                    //1 x, x = idx
                                    case 4:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //sync to all OTHER players
                                            if (i != player)
                                            {
                                                players[i].output.write(new byte[]{4, (byte) player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;

                                    //Spieler ist fertig mit seinem Zug
                                    //1 x, x = next player
                                    case 5:
                                        //calculate the next player
                                        currentPlayer++;
                                        if (currentPlayer >= players.length)
                                        {
                                            currentPlayer = 0;
                                        }
                                        //sync the activation of the current player to ALL clients
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            players[i].output.write(new byte[]{2, (byte) currentPlayer, 0, 0, 0, 0, 0, 0, 0, 0});
                                        }
                                        break;

                                        //Es wurde auf ein CoinField getreten (Rot oder Blau)
                                    //1 x, x = player; 2 y, y = Loose or Get
                                    case 6:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //sync to OTHER players
                                            if (i != player)
                                            {
                                                players[i].output.write(new byte[]{6, (byte)player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;

                                    //Eventstop (ItemShop, BuyStar, ...) ist fertig
                                    //1 x, x = player
                                    case 7:
                                        //sync to ALL players
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            players[i].output.write(new byte[]{7, (byte)player, 0, 0, 0, 0, 0, 0, 0, 0});
                                        }
                                        break;
                                        //Spieler hat einen Stern gekauft
                                    case 8:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //sync to OTHER players
                                            if (i != player)
                                            {
                                                players[i].output.write(new byte[]{8, (byte)player, 0, 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;

                                        //Spieler hat ein Item Gekauft
                                    //1 x, x = player; 2 y, y = Item.Type (byte)
                                    case 9:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //Sync to OTHER players
                                            if (i != player)
                                            {
                                                players[i].output.write(new byte[]{9, (byte)player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;
                                        //the timeout check action
                                    case 126:
                                        players[player].lastTimeSendTimeOutCheck = System.currentTimeMillis();
                                        break;
                                }

                            } catch (IOException e)
                            {
                                //if something is crashing, the room is being deleted
                                server.deleteRoom(roomCode);
                            }
                        }
                    }
                });
                //name the thread for better debugging
                t.setName("Thread Player: " + (playerCount - 1));
                //start the thread
                t.start();
                //add the thread to the threads list
                threads.add(t);
                break;
            }
        }

        //Make clients add all players to the world
        if (playerCount == players.length)
        {
            for (int j = 0; j < players.length; j++)
            {
                for (int k = 0; k < players.length; k++)
                {
                    //0 1 create new player
                    //1 0 no playable player
                    //1 1 playable character
                    //2-3-4 default pos
                    //5 player index

                    //if the player should be playable
                    if (j == k)
                    {
                        try
                        {
                            //sends the inital message to the player
                            players[j].output.write(new byte[]{1, 1, (byte) startPositions[k].x, (byte) startPositions[k].y, (byte) startPositions[k].z, (byte) k, 0, 0, 0, 0});

                            //sends the name of the player (ASCII encoding)............................................................................
                            byte[] name = players[k].name.substring(0, 8).getBytes(StandardCharsets.US_ASCII);
                            byte[] send = new byte[10];
                            send[0] = 5;
                            send[1] = (byte) k;

                            for (int i = 2; i < send.length; i++)
                            {
                                send[i] = name[i - 2];
                            }

                            players[j].output.write(send);
                            //stop sending the name................................................................................
                        } catch (IOException e)
                        {

                        }
                    } 
                    //if the player should be a bot
                    else
                    {
                        try
                        {
                            players[j].output.write(new byte[]{1, 0, (byte) startPositions[k].x, (byte) startPositions[k].y, (byte) startPositions[k].z, (byte) k, 0, 0, 0, 0});
                            byte[] name = players[k].name.substring(0, 8).getBytes(StandardCharsets.US_ASCII);
                            byte[] send = new byte[10];
                            send[0] = 5;
                            send[1] = (byte) k;

                            for (int i = 2; i < send.length; i++)
                            {
                                send[i] = name[i - 2];
                            }

                            players[j].output.write(send);
                        } catch (IOException e)
                        {

                        }
                    }
                }
            }

            //0 2, activate a player
            //1 x, x = idx

            //send the inital activation message
            try
            {
                for (Client c : players)
                {
                    c.output.write(new byte[]{2, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                }
            } catch (IOException e)
            {

            }
        }
    }

    /**
     * Closes the room
     */
    public void closeRoom()
    {
        //try to cick all the players (still connected) of the room
        try
        {
            
            if (players[0] != null)
            {
                //127 is the action for room cick
                players[0].output.write(new byte[]{127, 0, 0, 0, 0, 0, 0, 0, 0, 0});
            }

        } catch (Exception ex)
        {
        }

        try
        {
            if (players[1] != null)
            {
                players[1].output.write(new byte[]{127, 0, 0, 0, 0, 0, 0, 0, 0, 0});
            }

        } catch (Exception ex)
        {
        }

        try
        {
            if (players[2] != null)
            {
                players[2].output.write(new byte[]{127, 0, 0, 0, 0, 0, 0, 0, 0, 0});
            }

        } catch (Exception ex)
        {
        }

        try
        {
            if (players[3] != null)
            {
                players[3].output.write(new byte[]{127, 0, 0, 0, 0, 0, 0, 0, 0, 0});
            }

        } catch (Exception ex)
        {
        }


        //try to close all open threads (not crashed yet)
        for (int i = 0; i < threads.size(); i++)
        {
            try
            {
                threads.get(i).interrupt();
            } catch (Exception e)
            {
            }
        }

        try
        {
            t.cancel();
        } catch (Exception e)
        {
        }
    }
}
