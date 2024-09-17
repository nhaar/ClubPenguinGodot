using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
    public partial class StartButton : Button
    {
        private void OnPressed()
        {
            Node2D mainMenu = (Node2D)GetParent();
            Node2D instructionMenu = (Node2D)mainMenu.GetNode("../ThinIceInstructionMenu");
            instructionMenu.Visible = true;
            mainMenu.QueueFree();
        }
    }
}

