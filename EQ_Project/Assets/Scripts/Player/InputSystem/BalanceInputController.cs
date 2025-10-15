using UnityEngine;
using CB.Core;      // GameModeController
using CB.Balance;   // BalanceSessionController

public class BalanceInputController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GameModeController gameMode;
    [SerializeField] BalanceSessionController session;

    [Header("Audio opcional")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip sfxInc;
    [SerializeField] AudioClip sfxDec;
    [SerializeField] AudioClip sfxMove;
    [SerializeField] AudioClip sfxOk;
    [SerializeField] AudioClip sfxError;

    int selSide = 0;   // 0 = izquierda, 1 = derecha
    int selIndex = 0;  // índice dentro del lado

    void Reset()
    {
        if (gameMode == null) gameMode = FindObjectOfType<GameModeController>();
        if (session == null) session = GetComponent<BalanceSessionController>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // al entrar, alinea la selección con el primer término válido
        SnapSelection();
        Render();
    }

    void Update()
    {
        if (gameMode == null || gameMode.State != GameState.Balance) return;
        if (session == null) return;

        // Navegación horizontal: A/D
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }

        // Ajuste de coeficiente: W/S
        if (Input.GetKeyDown(KeyCode.W))
        {
            session.Adjust(selSide, selIndex, +1);
            Play(sfxInc);
            Render();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            session.Adjust(selSide, selIndex, -1);
            Play(sfxDec);
            Render();
        }

        // Verificar: Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool ok = session.IsBalancedNow();
            if (ok)
            {
                Play(sfxOk);
                // aquí luego haremos el “pasar nivel” o “confirmar balanceo”
            }
            else
            {
                session.errorCount++;
                Play(sfxError);
                // aquí luego dispararemos feedback visual (parpadeo rojo, etc.)
            }
        }

        // Salir del modo balance: Esc o E
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
        {
            gameMode.ExitBalance();
        }
    }

    void MoveLeft()
    {
        int lCount = session.LeftCount;
        int rCount = session.RightCount;

        if (selSide == 1 && rCount > 0)
        {
            // estamos en derecha: retroceder dentro de derecha o saltar a izquierda
            if (selIndex > 0) selIndex--;
            else
            {
                selSide = 0;
                selIndex = Mathf.Max(0, lCount - 1);
            }
        }
        else if (selSide == 0 && lCount > 0)
        {
            // estamos en izquierda
            selIndex = Mathf.Max(0, selIndex - 1);
        }
        Play(sfxMove);
        Render();
    }

    void MoveRight()
    {
        int lCount = session.LeftCount;
        int rCount = session.RightCount;

        if (selSide == 0 && lCount > 0)
        {
            // estamos en izquierda: avanzar o saltar a derecha
            if (selIndex < lCount - 1) selIndex++;
            else
            {
                selSide = 1;
                selIndex = 0;
            }
        }
        else if (selSide == 1 && rCount > 0)
        {
            // estamos en derecha
            selIndex = Mathf.Min(rCount - 1, selIndex + 1);
        }
        Play(sfxMove);
        Render();
    }

    void SnapSelection()
    {
        int lCount = session.LeftCount;
        int rCount = session.RightCount;

        if (lCount > 0)
        {
            selSide = 0;
            selIndex = Mathf.Clamp(selIndex, 0, lCount - 1);
        }
        else
        {
            selSide = 1;
            selIndex = Mathf.Clamp(selIndex, 0, Mathf.Max(0, rCount - 1));
        }
    }

    void Render()
    {
        session.Render(selSide, selIndex);
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
