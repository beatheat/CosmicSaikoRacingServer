using System.Text.Json.Serialization;

namespace CSRServer.Game
{
	internal class Buff
	{
		public enum Type
		{
			ELECTRIC_LEAK, PROLIFERATION, EXPOSURE, BREAK_DOWN, HIGH_EFFICIENCY, LOW_EFFICIENCY
		}
		public Type type { protected set; get; }
		public int count;
		public Dictionary<string, object> variables { private set; get; }
		
		[JsonIgnore]
		public BuffEffect initEffect;
		
		public Buff(Type type, BuffEffect initEffect)
		{
			this.type = type;
			this.initEffect = initEffect;
			this.count = 1;
			this.variables = new Dictionary<string, object>();
		}

		public void Init(GamePlayer player)
		{
			initEffect(this, player);
		}

		public void Release()
		{
			count = 0;
		}
		
		public T? GetVariable<T>(string key)
		{
			if (variables.ContainsKey(key))
				return (T) variables[key];
			return default(T);
		}

		public Buff Clone()
		{
			return (Buff)this.MemberwiseClone();
		}
		
	}
}