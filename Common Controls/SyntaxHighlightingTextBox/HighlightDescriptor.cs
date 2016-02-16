using System;
using System.Drawing;

namespace DamirM.Controls
{
	public class HighlightDescriptor
	{
        public readonly Color Color;
        public readonly Font Font;
        public readonly string Token;
        public readonly string CloseToken;
        public readonly string TokenLeftCondition;
        public readonly string TokenRightCondition;
        public readonly DescriptorType DescriptorType;
        public readonly DescriptorRecognition DescriptorRecognition; 

		public HighlightDescriptor(string token, Color color, Font font, DescriptorType descriptorType, DescriptorRecognition dr)
		{
			if (descriptorType == DescriptorType.ToCloseToken)
			{
				throw new ArgumentException("You may not choose ToCloseToken DescriptorType without specifing an end token.");
			}
			Color = color;
			Font = font;
			Token = token;
			DescriptorType = descriptorType;
			DescriptorRecognition = dr;
			CloseToken = null;
		}

        /// <summary>
        /// For DescriptorProccessModes.ProcsessLineForDescriptor use this constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokenLeftCondition"></param>
        /// <param name="tokenRightCondition"></param>
        /// <param name="color"></param>
        /// <param name="font"></param>
        public HighlightDescriptor(string token, string tokenLeftCondition, string tokenRightCondition, Color color, Font font)
        {
            Color = color;
            Font = font;
            Token = token;
            TokenLeftCondition = tokenLeftCondition;
            TokenRightCondition = tokenRightCondition;
            // default values, not in use for DescriptorProccessModes.ProcsessLineForDescriptor
            DescriptorType = DescriptorType.Word;
            DescriptorRecognition = DescriptorRecognition.Contains;
            CloseToken = null;
        }

		public HighlightDescriptor(string token, string closeToken, Color color, Font font, DescriptorType descriptorType, DescriptorRecognition dr)
		{
			Color = color;
			Font = font;
			Token = token;
			DescriptorType = descriptorType;
			CloseToken = closeToken;
			DescriptorRecognition = dr;
		}
    }

	
	public enum DescriptorType
	{
		/// <summary>
		/// Causes the highlighting of a single word
		/// </summary>
		Word,
        /// <summary>
        /// Causes the highlighting of a single word
        /// </summary>
        WordMatch,
		/// <summary>
		/// Causes the entire line from this point on the be highlighted, regardless of other tokens
		/// </summary>
		ToEOL,
		/// <summary>
		/// Highlights all text until the end token;
		/// </summary>
		ToCloseToken
	}

	public enum DescriptorRecognition
	{
		/// <summary>
		/// Only if the whole token is equal to the word
		/// </summary>
		WholeWord,
		/// <summary>
		/// If the word starts with the token
		/// </summary>
		StartsWith,
		/// <summary>
		/// If the word contains the Token
		/// </summary>
		Contains,
		/// <summary>
        /// If the word is regular expresion
		/// </summary>
		RegEx
	}
}
