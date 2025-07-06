using System;
using System.Collections.Generic;

namespace Pixel_Wall_E
{
    public enum TokenType
    {
        // Comandos básicos
        Spawn,
        Color,
        Size,
        DrawLine,
        DrawCircle,
        DrawRectangle,
        Fill,

        // Funciones
        GetActualX,
        GetActualY,
        GetCanvasSize,
        GetColorCount,
        IsBrushColor,
        IsBrushSize,
        IsCanvasColor,

        // Estructuras de control
        If,
        Then,
        Else,
        EndIf,
        While,
        Do,
        EndWhile,
        Goto,
        Label,

        // Valores literales
        Number,
        String,
        Identifier,
        True,
        False,

        // Operadores
        Plus,
        Minus,
        Multiply,
        Divide,
        Modulo,
        Power,

        // Comparación
        Equal,
        NotEqual,
        LessThan,
        GreaterThan,
        LessOrEqual,
        GreaterOrEqual,

        // Lógicos
        And,
        Or,
        Not,

        // Asignación
        Assign,

        // Símbolos
        LeftParen,
        RightParen,
        LeftBracket,
        RightBracket,
        Comma,
        Semicolon,
        Colon,

        // Control
        NewLine,
        EOF,
        Comment
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int LineNumber { get; }
        public int ColumnNumber { get; }

        public Token(TokenType type, string value, int lineNumber, int columnNumber)
        {
            Type = type;
            Value = value;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public override string ToString() => $"{Type} '{Value}' (Line {LineNumber}, Col {ColumnNumber})";
    }

    public class Lexer
    {
        private readonly string _source;
        private int _position;
        private int _lineNumber = 1;
        private int _columnNumber = 1;

        public Lexer(string source)
        {
            _source = source;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _source.Length)
            {
                char current = Peek();

                if (char.IsWhiteSpace(current))
                {
                    if (current == '\n')
                    {
                        tokens.Add(new Token(TokenType.NewLine, "\\n", _lineNumber, _columnNumber));
                        _lineNumber++;
                        _columnNumber = 0; // Reset to 0 (se incrementará a 1 después)
                    }
                    _position++;
                    _columnNumber++;
                    continue;
                }

                if (current == '/' && PeekNext() == '/')
                {
                    tokens.Add(ReadComment());
                    continue;
                }

                if (char.IsDigit(current))
                {
                    tokens.Add(ReadNumber());
                    continue;
                }

                if (char.IsLetter(current) || current == '_')
                {
                    tokens.Add(ReadIdentifierOrKeyword());
                    continue;
                }

                if (current == '"')
                {
                    tokens.Add(ReadString());
                    continue;
                }

                tokens.Add(ReadSymbolOrOperator());
            }

            tokens.Add(new Token(TokenType.EOF, "", _lineNumber, _columnNumber));
            return tokens;
        }

        private char Peek() => _position < _source.Length ? _source[_position] : '\0';
        private char PeekNext() => _position + 1 < _source.Length ? _source[_position + 1] : '\0';

        private Token ReadComment()
        {
            int startColumn = _columnNumber;
            _position += 2; // Salta "//"
            _columnNumber += 2;

            int start = _position;
            while (_position < _source.Length && Peek() != '\n')
            {
                _position++;
                _columnNumber++;
            }

            return new Token(TokenType.Comment, _source.Substring(start, _position - start), _lineNumber, startColumn);
        }

        private Token ReadNumber()
        {
            int start = _position;
            int startColumn = _columnNumber;

            while (_position < _source.Length && char.IsDigit(Peek()))
            {
                _position++;
                _columnNumber++;
            }

            return new Token(TokenType.Number, _source.Substring(start, _position - start), _lineNumber, startColumn);
        }

        private Token ReadIdentifierOrKeyword()
        {
            int start = _position;
            int startColumn = _columnNumber;

            while (_position < _source.Length && (char.IsLetterOrDigit(Peek()) || Peek() == '_' || Peek() == ':'))
            {
                _position++;
                _columnNumber++;
            }

            string value = _source.Substring(start, _position - start);

            if (value.EndsWith(":"))
            {
                string label = value.TrimEnd(':');
                if (!string.IsNullOrWhiteSpace(label))
                {
                    return new Token(TokenType.Label, label, _lineNumber, startColumn);
                }
            }

            TokenType type = value.ToLower() switch
            {
                "spawn" => TokenType.Spawn,
                "color" => TokenType.Color,
                "size" => TokenType.Size,
                "drawline" => TokenType.DrawLine,
                "drawcircle" => TokenType.DrawCircle,
                "drawrectangle" => TokenType.DrawRectangle,
                "fill" => TokenType.Fill,
                "getactualx" => TokenType.GetActualX,
                "getactualy" => TokenType.GetActualY,
                "getcanvassize" => TokenType.GetCanvasSize,
                "getcolorcount" => TokenType.GetColorCount,
                "isbrushcolor" => TokenType.IsBrushColor,
                "isbrushsize" => TokenType.IsBrushSize,
                "iscanvascolor" => TokenType.IsCanvasColor,
                "if" => TokenType.If,
                "then" => TokenType.Then,
                "else" => TokenType.Else,
                "endif" => TokenType.EndIf,
                "while" => TokenType.While,
                "do" => TokenType.Do,
                "endwhile" => TokenType.EndWhile,
                "goto" => TokenType.Goto,
                "true" => TokenType.True,
                "false" => TokenType.False,
                "and" => TokenType.And,
                "or" => TokenType.Or,
                "not" => TokenType.Not,
                _ => TokenType.Identifier
            };

            return new Token(type, value, _lineNumber, startColumn);
        }

        private Token ReadString()
        {
            int startColumn = _columnNumber;
            _position++; // Salta la comilla inicial
            _columnNumber++;

            int start = _position;
            bool escape = false;

            while (_position < _source.Length)
            {
                if (escape)
                {
                    escape = false;
                    _position++;
                    _columnNumber++;
                    continue;
                }

                if (Peek() == '\\')
                {
                    escape = true;
                    _position++;
                    _columnNumber++;
                    continue;
                }

                if (Peek() == '"')
                    break;

                _position++;
                _columnNumber++;
            }

            if (_position >= _source.Length)
                throw new Exception($"Cadena sin terminar en línea {_lineNumber}");

            string value = _source.Substring(start, _position - start);
            _position++; // Salta la comilla final
            _columnNumber++;

            return new Token(TokenType.String, value, _lineNumber, startColumn);
        }

        private Token ReadSymbolOrOperator()
        {
            char current = Peek();
            int startColumn = _columnNumber;
            _position++;
            _columnNumber++;

            switch (current)
            {
                case '(': return new Token(TokenType.LeftParen, "(", _lineNumber, startColumn);
                case ')': return new Token(TokenType.RightParen, ")", _lineNumber, startColumn);
                case '[': return new Token(TokenType.LeftBracket, "[", _lineNumber, startColumn);
                case ']': return new Token(TokenType.RightBracket, "]", _lineNumber, startColumn);
                case ',': return new Token(TokenType.Comma, ",", _lineNumber, startColumn);
                case ';': return new Token(TokenType.Semicolon, ";", _lineNumber, startColumn);
                case ':': return new Token(TokenType.Colon, ":", _lineNumber, startColumn);
                case '+': return new Token(TokenType.Plus, "+", _lineNumber, startColumn);
                case '-': return new Token(TokenType.Minus, "-", _lineNumber, startColumn);
                case '*':
                    if (Peek() == '*')
                    {
                        _position++;
                        _columnNumber++;
                        return new Token(TokenType.Power, "**", _lineNumber, startColumn);
                    }
                    return new Token(TokenType.Multiply, "*", _lineNumber, startColumn);
                case '/': return new Token(TokenType.Divide, "/", _lineNumber, startColumn);
                case '%': return new Token(TokenType.Modulo, "%", _lineNumber, startColumn);
                case '=':
                    if (Peek() == '=')
                    {
                        _position++;
                        _columnNumber++;
                        return new Token(TokenType.Equal, "==", _lineNumber, startColumn);
                    }
                    return new Token(TokenType.Assign, "=", _lineNumber, startColumn);
                case '!':
                    if (Peek() == '=')
                    {
                        _position++;
                        _columnNumber++;
                        return new Token(TokenType.NotEqual, "!=", _lineNumber, startColumn);
                    }
                    return new Token(TokenType.Not, "!", _lineNumber, startColumn);
                case '<':
                    if (Peek() == '=')
                    {
                        _position++;
                        _columnNumber++;
                        return new Token(TokenType.LessOrEqual, "<=", _lineNumber, startColumn);
                    }
                    else if (Peek() == '-')
                    {
                        _position++;
                        _columnNumber++;
                        return new Token(TokenType.Assign, "<-", _lineNumber, startColumn);
                    }
                    return new Token(TokenType.LessThan, "<", _lineNumber, startColumn);
                case '>':
                    if (Peek() == '=')
                    {
                        _position++;
                        _columnNumber++;
                        return new Token(TokenType.GreaterOrEqual, ">=", _lineNumber, startColumn);
                    }
                    return new Token(TokenType.GreaterThan, ">", _lineNumber, startColumn);
                case '&':
                    if (Peek() == '&')
                    {
                        _position++;
                        _columnNumber++;
                        return new Token(TokenType.And, "&&", _lineNumber, startColumn);
                    }
                    break;
                case '|':
                    if (Peek() == '|')
                    {
                        _position++;
                        _columnNumber++;
                        return new Token(TokenType.Or, "||", _lineNumber, startColumn);
                    }
                    break;
            }

            throw new Exception($"Carácter inesperado '{current}' en línea {_lineNumber}, columna {startColumn}");
        }
    }
}