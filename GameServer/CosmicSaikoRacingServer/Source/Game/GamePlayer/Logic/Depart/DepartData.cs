using CSR.Game.GameObject;

#pragma warning disable CS8618

namespace CSR.Game;

public delegate CardEffectModule.Result DepartEvent();

public class DepartData
{
	public Queue<DepartEvent> events;
}