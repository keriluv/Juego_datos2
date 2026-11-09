using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ValidadorTutorial
{
    private const string RutaEscena = "Assets/Scenes/TutorialGoogle.unity";

    public static void ValidarPorBatch()
    {
        EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("TUTORIAL_VALIDACION:");

        GameObject safe = GameObject.Find("Canvas/SafeArea");
        InteraccionTutorial inter = safe != null ? safe.GetComponent<InteraccionTutorial>() : null;
        sb.AppendLine("  InteraccionTutorial: " + (inter != null ? "OK" : "FALTA"));

        if (inter != null)
        {
            sb.AppendLine("  jugador: " + (inter.jugador != null));
            sb.AppendLine("  dialogo: " + (inter.dialogo != null));
            sb.AppendLine("  textoProgreso: " + (inter.textoProgreso != null));
            sb.AppendLine("  clippy: " + (inter.clippy != null));
            sb.AppendLine("  panelExtensiones: " + (inter.panelExtensiones != null));
            sb.AppendLine("  panelControles: " + (inter.panelControles != null));
            sb.AppendLine("  anuncio: " + (inter.anuncio != null));
            sb.AppendLine("  fondoPagina: " + (inter.fondoPagina != null));
            sb.AppendLine("  botonExtensiones: " + (inter.botonExtensiones != null));
            sb.AppendLine("  botonCerrar: " + (inter.botonCerrar != null));
            sb.AppendLine("  botonBuscar: " + (inter.botonBuscar != null));
            sb.AppendLine("  botonSuerte: " + (inter.botonSuerte != null));
            sb.AppendLine("  filas: " + inter.filas.Count);
            sb.AppendLine("  extensiones: " + inter.extensiones.Count);
            sb.AppendLine("  accesosBotones: " + inter.accesosBotones.Count);
            sb.AppendLine("  campoBusqueda: " + (inter.campoBusqueda != null));
            sb.AppendLine("  panelResultados: " + (inter.panelResultados != null));
            sb.AppendLine("  tituloResultados: " + (inter.tituloResultados != null));
            sb.AppendLine("  botonCerrarResultados: " + (inter.botonCerrarResultados != null));
            sb.AppendLine("  textoUrl: " + (inter.textoUrl != null));
            sb.AppendLine("  resultados: " + inter.resultados.Count);

            int resultadosOk = 0;
            for (int i = 0; i < inter.resultados.Count; i++)
            {
                ResultadoBusquedaUI r = inter.resultados[i];
                if (r != null && r.titulo != null && r.url != null && r.resumen != null && r.boton != null)
                    resultadosOk++;
            }
            sb.AppendLine("  resultados conectados: " + resultadosOk);

            int conManager = 0;
            for (int i = 0; i < inter.filas.Count; i++)
                if (inter.filas[i] != null && inter.filas[i].manager == inter) conManager++;
            sb.AppendLine("  filas conectadas: " + conManager);

            int extensionesOk = 0;
            for (int i = 0; i < inter.extensiones.Count; i++)
                if (inter.extensiones[i] != null && inter.extensiones[i].esExtension) extensionesOk++;
            sb.AppendLine("  plataformas de extension: " + extensionesOk);
        }

        GameObject jugadorGO = GameObject.Find("Canvas/SafeArea/Mundo/Jugador");
        JugadorTutorial jugador = jugadorGO != null ? jugadorGO.GetComponent<JugadorTutorial>() : null;
        sb.AppendLine("  JugadorTutorial: " + (jugador != null));
        if (jugador != null)
        {
            sb.AppendLine("  jugador.mundo: " + (jugador.mundo != null));
            sb.AppendLine("  jugador.sprite: " + (jugador.sprite != null));
            sb.AppendLine("  posicion inicial: " + jugador.posicionInicial);
            sb.AppendLine("  posicion actual: " + ((RectTransform)jugadorGO.transform).anchoredPosition);
        }

        sb.AppendLine("  plataformas activas: " + PlataformaUI.Todas.Count);

        int botonSaltar = GameObject.Find("Canvas/SafeArea/BotonSaltar")?.GetComponent<BotonSaltarTutorial>() != null ? 1 : 0;
        sb.AppendLine("  BotonSaltarTutorial: " + botonSaltar);

        GameObject tarjetaPaso3 = GameObject.Find("Canvas/SafeArea/PanelControles/TarjetaPaso3");
        sb.AppendLine("  control Golpear: " + (tarjetaPaso3 != null ? "OK" : "FALTA"));

        Debug.Log(sb.ToString());
    }
}
