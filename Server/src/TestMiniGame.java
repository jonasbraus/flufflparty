import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.Timer;
import java.util.TimerTask;

public class TestMiniGame extends Minigame
{
    public TestMiniGame(Room room) {
        super(room);
    }

    @Override
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

                //Buffer für eingehende narichten
                byte[] readData = new byte[10];


                //TEST
                try
                {
                    output.write(new byte[]{1, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                } catch (IOException e)
                {

                }

                //eingehende narichten verarbeiten:
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
}
