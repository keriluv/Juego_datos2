using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central logical progression (not a collectible object).
/// Extends the old ProgresoNiveles PlayerPrefs scheme and adds objectives/keys/milestones.
/// </summary>
public class GameProgressionManager : MonoBehaviour
{
    public static GameProgressionManager Instancia { get; private set; }

    public const string PrefijoNivel = "completado_";
    public const string PrefijoObjetivo = "objetivo_";
    public const string PrefijoKeys = "keys_";
    public const string PrefijoHito = "hito_";

    public static event System.Action OnProgressionChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Arrancar()
    {
        if (Instancia != null) return;
        GameObject go = new GameObject("GameProgressionManager");
        DontDestroyOnLoad(go);
        go.AddComponent<GameProgressionManager>();
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    public static GameProgressionManager Asegurar()
    {
        if (Instancia != null) return Instancia;
        Arrancar();
        return Instancia;
    }

    // ---- Levels ----

    public void RegisterLevelCompleted(string escena)
    {
        if (string.IsNullOrEmpty(escena)) return;
        if (IsLevelCompleted(escena)) return;

        PlayerPrefs.SetInt(PrefijoNivel + escena, 1);
        PlayerPrefs.Save();
        Notificar();
    }

    public bool IsLevelCompleted(string escena)
    {
        if (string.IsNullOrEmpty(escena)) return false;
        return PlayerPrefs.GetInt(PrefijoNivel + escena, 0) == 1;
    }

    // ---- Objectives ----

    public void RegisterObjectiveCompleted(string objetivoId)
    {
        if (string.IsNullOrEmpty(objetivoId)) return;
        if (IsObjectiveCompleted(objetivoId)) return;

        PlayerPrefs.SetInt(PrefijoObjetivo + objetivoId, 1);
        PlayerPrefs.Save();
        Notificar();
    }

    public bool IsObjectiveCompleted(string objetivoId)
    {
        if (string.IsNullOrEmpty(objetivoId)) return false;
        return PlayerPrefs.GetInt(PrefijoObjetivo + objetivoId, 0) == 1;
    }

    // ---- Keys / counters ----

    public void RegisterKeyCollected(string escena, int totalRequerido = 4)
    {
        if (string.IsNullOrEmpty(escena)) return;
        SetKeysCollected(escena, GetKeysCollected(escena) + 1, totalRequerido);
    }

    public void SetKeysCollected(string escena, int cantidad, int totalRequerido = 4)
    {
        if (string.IsNullOrEmpty(escena)) return;
        int anterior = GetKeysCollected(escena);
        int actual = Mathf.Max(0, cantidad);
        if (actual == anterior)
        {
            if (actual >= totalRequerido)
            {
                RegisterObjectiveCompleted("CollectAllKeys_" + escena);
                RegisterMilestone("KeysComplete_" + escena);
            }
            return;
        }

        PlayerPrefs.SetInt(PrefijoKeys + escena, actual);
        PlayerPrefs.Save();

        if (actual >= totalRequerido)
        {
            RegisterObjectiveCompleted("CollectAllKeys_" + escena);
            RegisterMilestone("KeysComplete_" + escena);
        }
        else
        {
            Notificar();
        }
    }

    public int GetKeysCollected(string escena)
    {
        if (string.IsNullOrEmpty(escena)) return 0;
        return PlayerPrefs.GetInt(PrefijoKeys + escena, 0);
    }

    // ---- Milestones ----

    public void RegisterMilestone(string hitoId)
    {
        if (string.IsNullOrEmpty(hitoId)) return;
        if (HasMilestone(hitoId)) return;

        PlayerPrefs.SetInt(PrefijoHito + hitoId, 1);
        PlayerPrefs.Save();
        Notificar();
    }

    public bool HasMilestone(string hitoId)
    {
        if (string.IsNullOrEmpty(hitoId)) return false;
        return PlayerPrefs.GetInt(PrefijoHito + hitoId, 0) == 1;
    }

    // ---- Requirement checks for map nodes ----
    // Formats: "level:SceneName", "objective:Id", "milestone:Id", "keys:SceneName:N", bare scene = level

    public bool CumpleRequisito(string requisito)
    {
        if (string.IsNullOrEmpty(requisito)) return true;

        string r = requisito.Trim();
        if (r.StartsWith("level:", StringComparison.OrdinalIgnoreCase))
            return IsLevelCompleted(r.Substring(6));
        if (r.StartsWith("objective:", StringComparison.OrdinalIgnoreCase))
            return IsObjectiveCompleted(r.Substring(10));
        if (r.StartsWith("milestone:", StringComparison.OrdinalIgnoreCase))
            return HasMilestone(r.Substring(10));
        if (r.StartsWith("keys:", StringComparison.OrdinalIgnoreCase))
        {
            string resto = r.Substring(5);
            string[] partes = resto.Split(':');
            string escena = partes[0];
            int need = partes.Length > 1 && int.TryParse(partes[1], out int n) ? n : 4;
            return GetKeysCollected(escena) >= need;
        }

        // Bare token: treat as completed level scene name.
        return IsLevelCompleted(r);
    }

    public bool CumpleTodos(IList<string> requisitos)
    {
        if (requisitos == null || requisitos.Count == 0) return true;
        for (int i = 0; i < requisitos.Count; i++)
        {
            if (!CumpleRequisito(requisitos[i])) return false;
        }
        return true;
    }

    public void ReiniciarTodo()
    {
        string[] escenas =
        {
            "MainMenu", "LevelSelector", "LevelYoutube", "LevelYoutubev2", "LevelYoutubev3",
            "LevelSnapchat", "EscenaTiktok", "TutorialGoogle", "MapaNiveles3D"
        };

        foreach (string e in escenas)
        {
            PlayerPrefs.DeleteKey(PrefijoNivel + e);
            PlayerPrefs.DeleteKey(PrefijoKeys + e);
        }

        string[] objetivos =
        {
            "CrackBroken_LevelYoutubev2",
            "CollectAllKeys_LevelYoutubev3",
            "Entered_LevelYoutubev3"
        };
        foreach (string o in objetivos)
            PlayerPrefs.DeleteKey(PrefijoObjetivo + o);

        string[] hitos =
        {
            "KeysComplete_LevelYoutubev3",
            "YoutubeArcComplete"
        };
        foreach (string h in hitos)
            PlayerPrefs.DeleteKey(PrefijoHito + h);

        PlayerPrefs.Save();
        Notificar();
    }

    public void ClearLevelCompleted(string escena)
    {
        if (string.IsNullOrEmpty(escena)) return;
        PlayerPrefs.DeleteKey(PrefijoNivel + escena);
        PlayerPrefs.Save();
        Notificar();
    }

    private void Notificar()
    {
        OnProgressionChanged?.Invoke();
    }
}

/// <summary>
/// Backward-compatible static API used by map scripts.
/// </summary>
public static class ProgresoNiveles
{
    public static void Completar(string escena)
    {
        GameProgressionManager.Asegurar().RegisterLevelCompleted(escena);
    }

    public static bool EstaCompletado(string escena)
    {
        return GameProgressionManager.Asegurar().IsLevelCompleted(escena);
    }

    public static void Reiniciar(string escena)
    {
        GameProgressionManager.Asegurar().ClearLevelCompleted(escena);
    }
}
