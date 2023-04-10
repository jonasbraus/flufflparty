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
    public Client[] players = new Client[2];
    //the array of players start positions
    private Vector3[] startPositions;
    //the generated roomcode
    public String roomCode;
    //the instance to the main server
    private Server server;

    //all threads created stored in this list to close them after room closure
    private List<Thread> threads = new ArrayList<>();

    //count of players in the game
    public int playerCount = 0;

    //tells players turn (array index)
    private int currentPlayer = 0;


    public Room(String roomCode, Server server)
    {

        this.roomCode = roomCode;
        startPositions = new Vector3[4];

        //set up the start positions
        startPositions[0] = new Vector3(96, 0, 126);
        startPositions[1] = new Vector3(92, 0, 126);
        startPositions[2] = new Vector3(96, 0, 128);
        startPositions[3] = new Vector3(92, 0, 128);

        this.server = server;
    }

    /**
     * Adds a Client to the game (on new player joined the room)
     * @param client
     */
    public void addClient(Client client)
    {

        //loops through all players existing and sends the needed information
        for (int i = 0; i < players.length; i++)
        {
            //places the new joined player in the next free slot
            if (players[i] == null)
            {
                players[i] = client;
                playerCount++;

                //LOG
                System.out.println(client.name.replace(" ", "") + " joined room " + roomCode + " successfully as player " + i);

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
                            server.deleteRoom(roomCode);
                        }

                        while (true)
                        {
                            try
                            {
                                //this is a buffer for all data being received by the player
                                byte[] readData = new byte[10];

                                //read the data to the buffer
                                input.read(readData);


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

                                    /**
                                     * //spieler hat gewürfelt
                                     */
                                    case 3:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //sync to all OTHER players
                                            if (i != player)
                                            {
                                                //!!data processing!!
                                                //----------------------------action---player-------rolled number----free space--------
                                                players[i].output.write(new byte[]{3, (byte) player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;
                                    /**
                                     * //pfeil wurde ausgewählt
                                     */
                                    case 4:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //!!data processing!!
                                            //sync to all OTHER players
                                            if (i != player)
                                            {
                                                //-------------------------------action----player-----arrow index--------free space-----
                                                players[i].output.write(new byte[]{4, (byte) player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;

                                    /**
                                     * //Spieler ist fertig mit seinem Zug
                                     */
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
                                            //!!data processing!!
                                            //----------------------------action--------the next player-----------free space------
                                            players[i].output.write(new byte[]{2, (byte) currentPlayer, 0, 0, 0, 0, 0, 0, 0, 0});
                                        }
                                        break;

                                    /**
                                     * //Es wurde auf ein CoinField getreten (Rot oder Blau)
                                     */
                                    case 6:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //sync to OTHER players
                                            if (i != player)
                                            {
                                                //!!data processing!!
                                                //-----------------------------action--player-------losse or get------------free space--
                                                players[i].output.write(new byte[]{6, (byte)player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;

                                    /**
                                     * //Eventstop (ItemShop, BuyStar, ...) ist fertig
                                     */
                                    case 7:
                                        //sync to ALL players
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //!!simple sync!!
                                            //------------------------------action---player---------free space--------
                                            players[i].output.write(new byte[]{7, (byte)player, 0, 0, 0, 0, 0, 0, 0, 0});
                                        }
                                        break;
                                    /**
                                     * //Spieler hat einen Stern gekauft
                                     */
                                    case 8:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //!!simple sync!!
                                            //sync to OTHER players
                                            if (i != player)
                                            {
                                                //-----------------------------action----player----------free space------
                                                players[i].output.write(new byte[]{8, (byte)player, 0, 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;

                                    /**
                                     * //Spieler hat ein Item Gekauft
                                     */
                                    case 9:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //Sync to OTHER players
                                            if (i != player)
                                            {
                                                //!!data processing!!
                                                //------------------------------action---player-------item ID--------free space--------
                                                players[i].output.write(new byte[]{9, (byte)player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;

                                    case 10:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            //Sync to OTHER players
                                            if (i != player)
                                            {
                                                //!!data processing!!
                                                //------------------------------action---player-------item index--------free space--------
                                                players[i].output.write(new byte[]{10, (byte)player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;
                                    case 126:
                                        server.deleteRoom(roomCode);
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
                            players[j].output.write(new byte[]{1, 1, (byte) startPositions[k].x, (byte) startPositions[k].y, (byte) startPositions[k].z, (byte) k, 0, 0, 0, (byte)players[k].characterID});

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
                            server.deleteRoom(roomCode);
                        }
                    } 
                    //if the player should be a bot
                    else
                    {
                        try
                        {
                            players[j].output.write(new byte[]{1, 0, (byte) startPositions[k].x, (byte) startPositions[k].y, (byte) startPositions[k].z, (byte) k, 0, 0, 0, (byte)players[k].characterID});
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
                            server.deleteRoom(roomCode);
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
                server.deleteRoom(roomCode);
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
                players[0].close();
            }

        } catch (Exception ex)
        {
        }

        try
        {
            if (players[1] != null)
            {
                players[1].output.write(new byte[]{127, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                players[1].close();
            }

        } catch (Exception ex)
        {
        }

        try
        {
            if (players[2] != null)
            {
                players[2].output.write(new byte[]{127, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                players[2].close();
            }

        } catch (Exception ex)
        {
        }

        try
        {
            if (players[3] != null)
            {
                players[3].output.write(new byte[]{127, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                players[3].close();
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
                System.out.println("thread " + threads.get(i).getName() + " stopped!");
            } catch (Exception e)
            {
            }
        }

        threads = null;
        players = null;
        startPositions = null;
    }
}
