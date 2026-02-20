using UnityEngine;
using System;

public class BalanceSelectionController : MonoBehaviour
{
    public int LeftSlots { get; private set; }
    public int RightSlots { get; private set; }

    // 0 = izquierda, 1 = derecha
    public int SelectedSide { get; private set; }
    public int SelectedIndex { get; private set; }

    public event Action<int, int> OnSelectionChanged;

    // -------------------------
    // Configuración
    // -------------------------
    public void Configure(int leftSlots, int rightSlots)
    {
        LeftSlots = Mathf.Max(0, leftSlots);
        RightSlots = Mathf.Max(0, rightSlots);

        Snap();
        Notify();
    }

    // -------------------------
    // Navegación
    // -------------------------
    public void MoveLeft()
    {
        if (SelectedSide == 1 && RightSlots > 0)
        {
            if (SelectedIndex > 0)
            {
                SelectedIndex--;
            }
            else
            {
                SelectedSide = 0;
                SelectedIndex = Mathf.Max(0, LeftSlots - 1);
            }
        }
        else if (SelectedSide == 0 && LeftSlots > 0)
        {
            SelectedIndex = Mathf.Max(0, SelectedIndex - 1);
        }

        Notify();
    }

    public void MoveRight()
    {
        if (SelectedSide == 0 && LeftSlots > 0)
        {
            if (SelectedIndex < LeftSlots - 1)
            {
                SelectedIndex++;
            }
            else
            {
                SelectedSide = 1;
                SelectedIndex = 0;
            }
        }
        else if (SelectedSide == 1 && RightSlots > 0)
        {
            SelectedIndex = Mathf.Min(RightSlots - 1, SelectedIndex + 1);
        }

        Notify();
    }

    // -------------------------
    // Helpers
    // -------------------------
    void Snap()
    {
        // Prioridad: lado izquierdo si existe
        if (LeftSlots > 0)
        {
            SelectedSide = 0;
            SelectedIndex = Mathf.Clamp(SelectedIndex, 0, LeftSlots - 1);
        }
        else if (RightSlots > 0)
        {
            SelectedSide = 1;
            SelectedIndex = Mathf.Clamp(SelectedIndex, 0, RightSlots - 1);
        }
        else
        {
            // No hay selección válida
            SelectedSide = 0;
            SelectedIndex = 0;
        }
    }

    void Notify()
    {
        OnSelectionChanged?.Invoke(SelectedSide, SelectedIndex);
    }
}
