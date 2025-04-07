
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public interface IWindowService
{
    void Close();

    void ToggleMoveableWindow();
    void ChangeMoveableWindowState(bool isMoveable);

    void SetWindowSize(double width, double height);
    void SetWindowPosition(double x, double y);
}

