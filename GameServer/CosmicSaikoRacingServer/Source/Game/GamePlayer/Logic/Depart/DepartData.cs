using CSR.Game.GameObject;

namespace CSR.Game.Player;


public delegate CardEffectModule.Result DepartEvent();

public class DepartData
{
	public Queue<DepartEvent> events;
}