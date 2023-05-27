namespace CSR.Game.GameObject;

public partial class Card
{
	/// <summary>
	/// 카드 내장 변수
	/// </summary>
	public class Variable
	{
		public int value;
		public int lowerBound;
		public int upperBound;

		public Variable Clone() => (Variable) this.MemberwiseClone();
	}

}