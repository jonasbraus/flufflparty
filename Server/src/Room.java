import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.net.ServerSocket;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

public class Room
{
    private Client[] players = new Client[3];
    private Vector3[] startPositions;
    private String roomCode;
    private Server server;

    private List<Thread> threads = new ArrayList<>();

    private int playerCount = 0;

    private int currentPlayer = 0;

    private long timeOut = 0;
    private Timer t;

    public Room(String roomCode, Server server)
    {
        timeOut = System.currentTimeMillis();

        this.roomCode = roomCode;
        startPositions = new Vector3[4];

        startPositions[0] = new Vector3(96, 1, 126);
        startPositions[1] = new Vector3(92, 1, 126);
        startPositions[2] = new Vector3(96, 1, 128);
        startPositions[3] = new Vector3(92, 1, 128);

        this.server = server;

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

    public void addClient(Client client)
    {
        timeOut = System.currentTimeMillis();

        for (int i = 0; i < players.length; i++)
        {
            if (players[i] == null)
            {
                players[i] = client;
                playerCount++;

                System.out.println("[" + Server.getDateTime() + "] " + client.name.replace(" ", "") + " joined room " + roomCode + " successfully as player " + i);

                Thread t = new Thread(new Runnable()
                {
                    @Override
                    public void run()
                    {
                        int player = playerCount - 1;

                        DataInputStream input = null;
                        try
                        {
                            input = new DataInputStream(players[player].socket.getInputStream());
                        } catch (IOException e)
                        {
                            throw new RuntimeException(e);
                        }

                        while (true)
                        {
                            try
                            {
                                byte[] readData = new byte[10];

                                input.read(readData);

                                timeOut = System.currentTimeMillis();

                                switch (readData[0])
                                {

                                    //spieler hat gewürfelt
                                    //1 x, x = zahl
                                    case 3:
                                        for (int i = 0; i < players.length; i++)
                                        {
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
                                            if (i != player)
                                            {
                                                players[i].output.write(new byte[]{4, (byte) player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;

                                    //Spieler ist fertig mit seinem Zug
                                    //1 x, x = next player
                                    case 5:
                                        currentPlayer++;
                                        if (currentPlayer >= players.length)
                                        {
                                            currentPlayer = 0;
                                        }
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            players[i].output.write(new byte[]{2, (byte) currentPlayer, 0, 0, 0, 0, 0, 0, 0, 0});
                                        }
                                        break;

                                    case 6:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            if (i != player)
                                            {
                                                players[i].output.write(new byte[]{6, (byte)player, readData[1], 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;

                                    case 7:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                                players[i].output.write(new byte[]{7, (byte)player, 0, 0, 0, 0, 0, 0, 0, 0});
                                        }
                                        break;
                                    case 8:
                                        for (int i = 0; i < players.length; i++)
                                        {
                                            if (i != player)
                                            {
                                                players[i].output.write(new byte[]{8, (byte)player, 0, 0, 0, 0, 0, 0, 0, 0});
                                            }
                                        }
                                        break;
                                    case 126:
                                        players[player].lastTimeSendTimeOutCheck = System.currentTimeMillis();
                                        break;
                                }

                            } catch (IOException e)
                            {
                                server.deleteRoom(roomCode);
                            }
                        }
                    }
                });
                t.setName("Thread Player: " + (playerCount - 1));
                t.start();
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
                    if (j == k)
                    {
                        try
                        {
                            players[j].output.write(new byte[]{1, 1, (byte) startPositions[k].x, (byte) startPositions[k].y, (byte) startPositions[k].z, (byte) k, 0, 0, 0, 0});
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
                    } else
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

    public void closeRoom()
    {
        try
        {

            if (players[0] != null)
            {
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
