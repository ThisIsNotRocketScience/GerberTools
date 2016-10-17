using System;
using System.Collections.Generic;
using System.Text;

namespace QuickFont
{
    public enum CharacterKerningRule
    {
        /// <summary>
        /// Ordinary kerning
        /// </summary>
        Normal,
        /// <summary>
        /// All kerning pairs involving this character will kern by 0. This will
        /// override both Normal and NotMoreThanHalf for any pair.
        /// </summary>
        Zero,
        /// <summary>
        /// Any kerning pairs involving this character will not kern
        /// by more than half the minimum width of the two characters 
        /// involved. This will override Normal for any pair.
        /// </summary>
        NotMoreThanHalf
    }


    public class QFontKerningConfiguration
    {
        /// <summary>
        /// Kerning rules for particular characters
        /// </summary>
        private Dictionary<char, CharacterKerningRule> CharacterKerningRules = new Dictionary<char, CharacterKerningRule>();

        /// <summary>
        /// When measuring the bounds of glyphs, and performing kerning calculations, 
        /// this is the minimum alpha level that is necessray for a pixel to be considered
        /// non-empty. This should be set to a value on the range [0,255]
        /// </summary>
        public byte alphaEmptyPixelTolerance = 0;


        /// <summary>
        /// Sets all characters in the given string to the specified kerning rule.
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="rule"></param>
        public void BatchSetCharacterKerningRule(String chars, CharacterKerningRule rule)
        {
            foreach (var c in chars)
            {
                CharacterKerningRules[c] = rule;
            }
        }

        /// <summary>
        /// Sets the specified character kerning rule.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="rule"></param>
        public void SetCharacterKerningRule(char c, CharacterKerningRule rule)
        {
            CharacterKerningRules[c] = rule;
        }

        public CharacterKerningRule GetCharacterKerningRule(char c)
        {
            if (CharacterKerningRules.ContainsKey(c))
            {
                return CharacterKerningRules[c];
            }

            return CharacterKerningRule.Normal;
        }

        /// <summary>
        /// Given a pair of characters, this will return the overriding 
        /// CharacterKerningRule.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public CharacterKerningRule GetOverridingCharacterKerningRuleForPair(String str)
        {

            if (str.Length < 2)
            {
                return CharacterKerningRule.Normal;
            }

            char c1 = str[0];
            char c2 = str[1];

            if (GetCharacterKerningRule(c1) == CharacterKerningRule.Zero || GetCharacterKerningRule(c2) == CharacterKerningRule.Zero)
            {
                return CharacterKerningRule.Zero;
            }
            else if (GetCharacterKerningRule(c1) == CharacterKerningRule.NotMoreThanHalf || GetCharacterKerningRule(c2) == CharacterKerningRule.NotMoreThanHalf)
            {
                return CharacterKerningRule.NotMoreThanHalf;
            }

            return CharacterKerningRule.Normal;
        }

        public QFontKerningConfiguration()
        {
            BatchSetCharacterKerningRule("_^", CharacterKerningRule.Zero);
            SetCharacterKerningRule('\"', CharacterKerningRule.NotMoreThanHalf);
            SetCharacterKerningRule('\'', CharacterKerningRule.NotMoreThanHalf);
        }
    }



}
