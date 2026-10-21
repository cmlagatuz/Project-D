// Project D | Codename: Zantetsuken — MiniJSON.cs (Editor-only compact JSON parser)
using System.Collections.Generic; using System.Globalization; using System.Text;
namespace ProjectD.EditorTools
{
    public static class MiniJSON
    {
        public static object Deserialize(string json){ if(json==null) return null; return Parser.Parse(json); }
        private sealed class Parser : System.IDisposable
        {
            private const string WordBreak="{}[],:\"";
            private System.IO.StringReader _json;
            private Parser(string s)=>_json=new System.IO.StringReader(s);
            public static object Parse(string s){ using var p=new Parser(s); return p.ParseValue(); }
            public void Dispose(){ _json.Dispose(); _json=null; }
            private Dictionary<string,object> ParseObject(){ var t=new Dictionary<string,object>(); _json.Read();
                while(true){ switch(NextToken){ case Token.None: return null; case Token.Comma: continue; case Token.CurlyClose: return t;
                    default: string n=ParseString(); if(n==null) return null; if(NextToken!=Token.Colon) return null; _json.Read(); t[n]=ParseValue(); break; } } }
            private List<object> ParseArray(){ var a=new List<object>(); _json.Read(); bool p=true;
                while(p){ Token tk=NextToken; switch(tk){ case Token.None: return null; case Token.Comma: continue; case Token.SquareClose: p=false; break; default: a.Add(ParseByToken(tk)); break; } } return a; }
            private object ParseValue()=>ParseByToken(NextToken);
            private object ParseByToken(Token tk)=>tk switch { Token.String=>ParseString(), Token.Number=>ParseNumber(), Token.CurlyOpen=>ParseObject(), Token.SquareOpen=>ParseArray(), Token.True=>true, Token.False=>false, Token.Null=>null, _=>null };
            private string ParseString(){ var s=new StringBuilder(); _json.Read(); bool p=true;
                while(p){ if(_json.Peek()==-1) break; char c=(char)_json.Read();
                    switch(c){ case '"': p=false; break; case '\\': if(_json.Peek()==-1){p=false;break;} char e=(char)_json.Read();
                        switch(e){ case '"': s.Append('"'); break; case '\\': s.Append('\\'); break; case '/': s.Append('/'); break; case 'b': s.Append('\b'); break; case 'f': s.Append('\f'); break; case 'n': s.Append('\n'); break; case 'r': s.Append('\r'); break; case 't': s.Append('\t'); break;
                            case 'u': var h=new char[4]; for(int i=0;i<4;i++) h[i]=(char)_json.Read(); s.Append((char)System.Convert.ToInt32(new string(h),16)); break; } break;
                        default: s.Append(c); break; } } return s.ToString(); }
            private object ParseNumber(){ string n=NextWord; if(n.IndexOf('.')==-1&&n.IndexOf('e')==-1&&n.IndexOf('E')==-1){ long.TryParse(n,out long l); return l; } double.TryParse(n,NumberStyles.Any,CultureInfo.InvariantCulture,out double d); return d; }
            private void EatWhitespace(){ while(char.IsWhiteSpace(PeekChar)){ _json.Read(); if(_json.Peek()==-1) break; } }
            private char PeekChar=>System.Convert.ToChar(_json.Peek());
            private char NextChar=>System.Convert.ToChar(_json.Read());
            private string NextWord { get { var w=new StringBuilder(); while(!IsWordBreak(PeekChar)){ w.Append(NextChar); if(_json.Peek()==-1) break; } return w.ToString(); } }
            private static bool IsWordBreak(char c)=>char.IsWhiteSpace(c)||WordBreak.IndexOf(c)!=-1;
            private Token NextToken { get { EatWhitespace(); if(_json.Peek()==-1) return Token.None;
                switch(PeekChar){ case '{': return Token.CurlyOpen; case '}': _json.Read(); return Token.CurlyClose; case '[': return Token.SquareOpen; case ']': _json.Read(); return Token.SquareClose; case ',': _json.Read(); return Token.Comma; case '"': return Token.String; case ':': return Token.Colon;
                    case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9': case '-': return Token.Number; }
                switch(NextWord){ case "false": return Token.False; case "true": return Token.True; case "null": return Token.Null; } return Token.None; } }
            private enum Token { None, CurlyOpen, CurlyClose, SquareOpen, SquareClose, Colon, Comma, String, Number, True, False, Null }
        }
    }
}
