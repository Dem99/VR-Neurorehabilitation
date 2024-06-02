namespace NeuroRehab.Core {
	public class CustomMessage {
		private readonly string messageVal = "";
		private readonly MessageType messageType = MessageType.NORMAL;
		private readonly bool localizeString = false;
		private readonly Arg[] args = null;

		public string MessageVal { get => messageVal; }
		public MessageType MessageType { get => messageType; }
		public bool LocalizeString { get => localizeString; }
		public Arg[] Args { get => args; }

		public CustomMessage(string messageVal, MessageType messageType, bool localizeString, Arg[] args) {
			this.messageVal = messageVal;
			this.messageType = messageType;
			this.localizeString = localizeString;
			this.args = args;
		}

		public CustomMessage(string messageVal, MessageType messageType, bool localizeString) {
			this.messageVal = messageVal;
			this.messageType = messageType;
			this.localizeString = localizeString;
		}

		public CustomMessage(string messageVal, MessageType messageType) {
			this.messageVal = messageVal;
			this.messageType = messageType;
		}

	}
}