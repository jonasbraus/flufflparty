import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

public class Minigame
{
    protected Room room;
    private List<Thread> threads = new ArrayList<>();
    private Client[] players;

    public Minigame(Room room)
    {
        this.room = room;
        players = new Client[room.players.length];
    }

    public void addClient(Client client, int index)
    {
        Thread t = new Thread(new Runnable()
        {
            @Override
            public void run()
            {
                players[index] = client;
                DataInputStream input = client.input;
                DataOutputStream output = client.output;
                byte[] readData = new byte[10];


                //TEST
                try
                {
                    output.write(new byte[]{1, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                } catch (IOException e)
                {

                }

                while (true)
                {
                    try
                    {
                        input.read(readData);

                        switch (readData[0])
                        {
                            //TEST
                            case 1:
                                System.out.println("testtttttttttttttt");


                                Timer t = new Timer();
                                TimerTask tt = new TimerTask()
                                {
                                    @Override
                                    public void run()
                                    {
                                        room.closeMinigame();
                                    }
                                };
                                t.schedule(tt, 1000);

                                break;
                                //TEST END
                        }
                    } catch (Exception e)
                    {

                    }
                }
            }
        });

        threads.add(t);
        t.start();
    }

    public void end()
    {
        for (int i = 0; i < threads.size(); i++)
        {
            try
            {
                threads.get(i).interrupt();
            } catch (Exception e)
            {
                e.printStackTrace();
            }
        }

        for(int i = 0; i < players.length; i++)
        {
            players[i].close();
        }

        threads.clear();
    }
}
