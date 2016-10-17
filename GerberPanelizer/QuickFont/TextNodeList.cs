using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;

namespace QuickFont
{


    enum TextNodeType { Word, LineBreak, Space }

    class TextNode
    {
        public TextNodeType Type;
        public string Text;
        public float Length; //pixel length (without tweaks)
        public float LengthTweak; //length tweak for justification

        public float ModifiedLength
        {
            get { return Length + LengthTweak; }
        }

        public TextNode(TextNodeType Type, string Text){
            this.Type = Type;
            this.Text = Text;
        }

        public TextNode Next;
        public TextNode Previous;

    }


    /// <summary>
    /// Class to hide TextNodeList and related classes from 
    /// user whilst allowing a textNodeList to be passed around.
    /// </summary>
    public class ProcessedText
    {
        internal TextNodeList textNodeList;
        internal float maxWidth;
        internal QFontAlignment alignment;
    }


    /// <summary>
    /// A doubly linked list of text nodes
    /// </summary>
    class TextNodeList : IEnumerable
    {
        public TextNode Head;
        public TextNode Tail;
       


        /// <summary>
        /// Builds a doubly linked list of text nodes from the given input string
        /// </summary>
        /// <param name="text"></param>
        public TextNodeList(string text)
        {

            
            #region parse text

            text = text.Replace("\r\n", "\r");

            bool wordInProgress = false;
            StringBuilder currentWord = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\r' || text[i] == '\n' || text[i] == ' ')
                {
                    if (wordInProgress)
                    {
                        Add(new TextNode(TextNodeType.Word, currentWord.ToString()));
                        wordInProgress = false;
                    }



                    if (text[i] == '\r' || text[i] == '\n')
                        Add(new TextNode(TextNodeType.LineBreak, null));
                    else if (text[i] == ' ')
                        Add(new TextNode(TextNodeType.Space, null));

                }
                else
                {
                    if (!wordInProgress)
                    {
                        wordInProgress = true;
                        currentWord = new StringBuilder();
                    }

                    currentWord.Append(text[i]);
                }

            }

            if (wordInProgress)
                Add(new TextNode(TextNodeType.Word, currentWord.ToString()));


            #endregion



        }

        public void MeasureNodes(QFontData fontData, QFontRenderOptions options){
            
            foreach(TextNode node in this){
                if(node.Length == 0f)
                    node.Length = MeasureTextNodeLength(node,fontData,options);
            }
        }



        private float MeasureTextNodeLength(TextNode node, QFontData fontData, QFontRenderOptions options)
        {

            bool monospaced = fontData.IsMonospacingActive(options);
            float monospaceWidth = fontData.GetMonoSpaceWidth(options);

            if (node.Type == TextNodeType.Space)
            {
                if (monospaced)
                    return monospaceWidth;

                return (float)Math.Ceiling(fontData.meanGlyphWidth * options.WordSpacing);
            }


            float length = 0f;
            if (node.Type == TextNodeType.Word)
            {
                
                for (int i = 0; i < node.Text.Length; i++)
                {
                    char c = node.Text[i];
                    if (fontData.CharSetMapping.ContainsKey(c))
                    {
                        if (monospaced)
                            length += monospaceWidth;
                        else
                            length += (float)Math.Ceiling(fontData.CharSetMapping[c].rect.Width + fontData.meanGlyphWidth * options.CharacterSpacing + fontData.GetKerningPairCorrection(i, node.Text, node));
                    }
                }
            }
            return length;
        }



        /// <summary>
        /// Splits a word into sub-words of size less than or equal to baseCaseSize 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="baseCaseSize"></param>
        public void Crumble(TextNode node, int baseCaseSize){

            //base case
            if(node.Text.Length <= baseCaseSize )
                return;
 
            var left = SplitNode(node);
            var right = left.Next;

            Crumble(left,baseCaseSize);
            Crumble(right,baseCaseSize);

        }


        /// <summary>
        /// Splits a word node in two, adding both new nodes to the list in sequence.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>The first new node</returns>
        public TextNode SplitNode(TextNode node)
        {
            if (node.Type != TextNodeType.Word)
                throw new Exception("Cannot slit text node of type: " + node.Type);

            int midPoint = node.Text.Length / 2;

            string newFirstHalf = node.Text.Substring(0, midPoint);
            string newSecondHalf = node.Text.Substring(midPoint, node.Text.Length - midPoint);


            TextNode newFirst = new TextNode(TextNodeType.Word, newFirstHalf);
            TextNode newSecond = new TextNode(TextNodeType.Word, newSecondHalf);
            newFirst.Next = newSecond;
            newSecond.Previous = newFirst;

            //node is head
            if (node.Previous == null)
                Head = newFirst;
            else
            {
                node.Previous.Next = newFirst;
                newFirst.Previous = node.Previous;
            }

            //node is tail
            if (node.Next == null)
                Tail = newSecond;
            else
            {
                node.Next.Previous = newSecond;
                newSecond.Next = node.Next;
            }

            return newFirst;
        }





        public void Add(TextNode node){

            //new node is head (and tail)
            if(Head == null){
                Head = node;
                Tail = node;
            } else {
                Tail.Next = node;
                node.Previous = Tail;
                Tail = node;
            }

        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();


           // for (var node = Head; node.Next != null; node = node.Next)

            foreach(TextNode node in this)
            {
                if (node.Type == TextNodeType.Space)
                    builder.Append(" ");
                if (node.Type == TextNodeType.LineBreak)
                    builder.Append(System.Environment.NewLine);
                if (node.Type == TextNodeType.Word)
                    builder.Append("" + node.Text + "");

            }

            return builder.ToString();
        }



        
        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return new TextNodeListEnumerator(this);
        }

        #endregion




        private class TextNodeListEnumerator : IEnumerator
        {
            private TextNode currentNode = null;
            private TextNodeList targetList;

            public TextNodeListEnumerator(TextNodeList targetList)
            {
                this.targetList = targetList;
            }

            public object Current
            {
                get { return currentNode; }
            }

            public virtual bool MoveNext()
            {
                if (currentNode == null)
                    currentNode = targetList.Head;
                else
                    currentNode = currentNode.Next;
                return currentNode != null;
            }

            public void Reset()
            {
                currentNode = null;
            }

            public void Dispose()
            {

            }

        }


    }





}
