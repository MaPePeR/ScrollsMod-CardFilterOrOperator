using System;
using ScrollsModLoader.Interfaces;
using Mono.Cecil;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
namespace CardFilterOrOperatorMod
{
	public class CardFilterOrOperatorMod : BaseMod
	{
		Type filterType;
		public CardFilterOrOperatorMod ()
		{
			filterType = typeof(DeckBuilder2).Assembly.GetType ("CardFilter+Filter");
		}

		public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version) {
			MethodDefinition cardFilterConstructor = scrollsTypes ["CardFilter"].Constructors.GetConstructor(false, new Type[] { "".GetType() });
			return new MethodDefinition[] {cardFilterConstructor};
		}
		public static string GetName() {
			return "CardFilterOr";
		}
		public static int GetVersion() {
			return 1;
		}

		public override void BeforeInvoke (InvocationInfo info) {
		}
		private char[] splitChars = new char[] { '|', ';' };
		FieldInfo filtersField = typeof(CardFilter).GetField ("filters", BindingFlags.NonPublic | BindingFlags.Instance);
		public override void AfterInvoke (InvocationInfo info, ref object returnValue) {
			string arg = (string)info.arguments [0];
			if (arg.IndexOfAny(splitChars) >= 0) {
				IList filters = (IList)filtersField.GetValue (info.target);

				string[] f =arg.Trim().Split (splitChars, StringSplitOptions.RemoveEmptyEntries);
				List<CardFilter> cardFilters = new List<CardFilter> ();
				foreach (string filter in f) {
					cardFilters.Add (new CardFilter (filter));
				}

				MyFilter orFilter = (Card c) => (
					cardFilters.Exists(
					    (CardFilter cardFilter) => (cardFilter.isIncluded(c))
					)
				);
				filters.Clear ();
				filters.Add(Cast(orFilter, filterType));
			}
		}



		//Method to cast a delegate - from http://code.logos.com/blog/2008/07/casting_delegates.html
		public static Delegate Cast(Delegate source, Type type)
		{
			if (source == null)
				return null;

			Delegate[] delegates = source.GetInvocationList();
			if (delegates.Length == 1)
				return Delegate.CreateDelegate(type,
				                               delegates[0].Target, delegates[0].Method);

			Delegate[] delegatesDest = new Delegate[delegates.Length];
			for (int nDelegate = 0; nDelegate < delegates.Length; nDelegate++)
				delegatesDest[nDelegate] = Delegate.CreateDelegate(type,
				                                                   delegates[nDelegate].Target, delegates[nDelegate].Method);
			return Delegate.Combine(delegatesDest);
		}
		private delegate bool MyFilter(Card c);
	}
}

