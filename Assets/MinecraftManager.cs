using UnityEngine;

public class MinecraftManager : MonoBehaviour
{
    private const string EscenaKeys = "LevelYoutubev3";
    private const int KeysRequeridas = 4;

    private int keys;

    private void Start()
    {
        keys = 0;
        GameProgressionManager prog = GameProgressionManager.Asegurar();
        // Visiting v3 is a milestone, not level completion.
        prog.RegisterMilestone("Entered_LevelYoutubev3");
        prog.RegisterObjectiveCompleted("Entered_LevelYoutubev3");
    }

    public void addcount()
    {
        keys++;
        GameProgressionManager.Asegurar().SetKeysCollected(EscenaKeys, keys, KeysRequeridas);

        if (keys >= KeysRequeridas)
            finishVideo();
    }

    public void finishVideo()
    {
        GameProgressionManager prog = GameProgressionManager.Asegurar();
        prog.RegisterLevelCompleted(EscenaKeys);
        prog.RegisterMilestone("YoutubeArcComplete");

        if (MultijugadorJuego.Instancia != null)
            MultijugadorJuego.Instancia.IrAEscena("LevelSelector");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelector");
    }
}
