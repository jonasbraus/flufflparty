import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

public abstract class Minigame
{
    //referenz auf den ursprünglichen raum
    protected Room room;

    //liste aller threads zum korrekten beenden dieser
    protected List<Thread> threads = new ArrayList<>();

    //liste aller temporären minigame verbindungen
    protected Client[] players;

    public Minigame(Room room)
    {
        this.room = room;
        players = new Client[room.players.length];
    }

    //fügt dem minigame "Raum" einen spieler hinzu (ähnlich wie im Room selbst)
    public abstract void addClient(Client client, int index);

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
