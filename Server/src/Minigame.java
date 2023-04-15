import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

public class Minigame
{
    //referenz auf den ursprünglichen raum
    protected Room room;

    //liste aller threads zum korrekten beenden dieser
    private List<Thread> threads = new ArrayList<>();

    //liste aller temporären minigame verbindungen
    private Client[] players;

    public Minigame(Room room)
    {
        this.room = room;
        players = new Client[room.players.length];
    }

    //fügt dem minigame "Raum" einen spieler hinzu (ähnlich wie im Room selbst)
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

    //das minigame wird beendet
    public void end()
    {
        //alle bestehenden threads in diesem Minigame beenden
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

        //temporäre verbindungen schließen
        for(int i = 0; i < players.length; i++)
        {
            players[i].close();
        }

        //threads liste löschen
        threads.clear();
    }
}
