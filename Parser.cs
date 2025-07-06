using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel_Wall_E
{
    public class Parser
    {
        /* Variables internas para tokens, posición actual, referencia al formulario
        y almacenamiento de variables, etiquetas y bucles */
        private readonly List<Token> _tokens;
        private int _current;
        private readonly Form1 _form;
        private readonly Dictionary<string, int> _variables = new Dictionary<string, int>();
        private readonly Dictionary<string, bool> _boolVariables = new Dictionary<string, bool>();
        private readonly Dictionary<string, int> _labels = new Dictionary<string, int>();
        private readonly Stack<int> _loopStack = new Stack<int>();

        // Constructor recibe lista de tokens y referencia al formulario
        public Parser(List<Token> tokens, Form1 form)
        {
            _tokens = tokens;
            _form = form;
        }

        // Método principal que inicia el parseo completo
        public void Parse()
        {
            while (!IsAtEnd())
            {
                if (Match(TokenType.Identifier) && Peek().Value.EndsWith(":"))
                {
                    string label = Previous().Value.TrimEnd(':');
                    _labels[label] = _current - 1; // Guardar posición del token
                }
                Advance();
            }

            _current = 0;
            while (!IsAtEnd())
            {
                try
                {
                    Statement();
                }
                catch (GotoException gotoEx)
                {
                    _current = gotoEx.TargetPosition;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en línea {_tokens[_current].LineNumber}: {ex.Message}");
                }
            }
        }

        // Parsear una sentencia según el token actual
        private void Statement()
        {
            if (Match(TokenType.Label))
            {
                Advance();
            }
            else if (Match(TokenType.Spawn)) SpawnStatement();
            else if (Match(TokenType.Color)) ColorStatement();
            else if (Match(TokenType.Size)) SizeStatement();
            else if (Match(TokenType.DrawLine)) DrawLineStatement();
            else if (Match(TokenType.DrawCircle)) DrawCircleStatement();
            else if (Match(TokenType.DrawRectangle)) DrawRectangleStatement();
            else if (Match(TokenType.Fill)) FillStatement();
            else if (Match(TokenType.Goto)) GotoStatement();
            else if (Match(TokenType.If)) IfStatement();
            else if (Match(TokenType.While)) WhileStatement();
            else if (Match(TokenType.Identifier) && Peek().Type == TokenType.Assign) Assignment();
            else if (Match(TokenType.Identifier) && Peek().Value.EndsWith(":")) Advance(); // Etiqueta
            else if (Match(TokenType.NewLine)) Advance(); // Ignorar saltos de línea
            else throw new Exception($"Declaración no válida: {Peek().Value}");
        }

        // Sentencias específicas para cada comando

        private void SpawnStatement()
        {
            Consume(TokenType.LeftParen, "Se esperaba '(' después de Spawn");
            int x = Expression();
            Consume(TokenType.Comma, "Se esperaba ',' entre coordenadas");
            int y = Expression();
            Consume(TokenType.RightParen, "Se esperaba ')' al final de Spawn");
            _form.ExecuteSpawn(x, y);
        }

        private void ColorStatement()
        {
            Consume(TokenType.LeftParen, "Se esperaba '(' después de Color");
            string color = Consume(TokenType.String, "Se esperaba nombre de color entre comillas").Value;
            Consume(TokenType.RightParen, "Se esperaba ')' al final de Color");
            _form.SetColor(color.Trim('"'));
        }

        private void SizeStatement()
        {
            Consume(TokenType.LeftParen, "Se esperaba '(' después de Size");
            int size = Expression();
            Consume(TokenType.RightParen, "Se esperaba ')' al final de Size");
            _form.SetBrushSize(size);
        }

        private void DrawLineStatement()
        {
            Consume(TokenType.LeftParen, "Se esperaba '(' después de DrawLine");
            int dirX = Expression();
            Consume(TokenType.Comma, "Se esperaba ',' después de dirección X");
            int dirY = Expression();
            Consume(TokenType.Comma, "Se esperaba ',' después de dirección Y");
            int distance = Expression();
            Consume(TokenType.RightParen, "Se esperaba ')' al final de DrawLine");
            _form.DrawLine(dirX, dirY, distance);
        }

        private void DrawCircleStatement()
        {
            Consume(TokenType.LeftParen, "Se esperaba '(' después de DrawCircle");
            int dirX = Expression();
            Consume(TokenType.Comma, "Se esperaba ',' después de dirección X");
            int dirY = Expression();
            Consume(TokenType.Comma, "Se esperaba ',' después de dirección Y");
            int radius = Expression();
            Consume(TokenType.RightParen, "Se esperaba ')' al final de DrawCircle");
            _form.DrawCircle(dirX, dirY, radius);
        }

        private void DrawRectangleStatement()
        {
            Consume(TokenType.LeftParen, "Se esperaba '(' después de DrawRectangle");
            int dirX = Expression();
            Consume(TokenType.Comma, "Se esperaba ',' después de dirección X");
            int dirY = Expression();
            Consume(TokenType.Comma, "Se esperaba ',' después de dirección Y");
            int distance = Expression();
            Consume(TokenType.Comma, "Se esperaba ',' después de distancia");
            int width = Expression();
            Consume(TokenType.Comma, "Se esperaba ',' después de ancho");
            int height = Expression();
            Consume(TokenType.RightParen, "Se esperaba ')' al final de DrawRectangle");
            _form.DrawRectangle(dirX, dirY, distance, width, height);
        }

        private void GotoStatement()
        {
            Consume(TokenType.LeftBracket, "Se esperaba '[' después de Goto");
            string label = Consume(TokenType.Identifier, "Se esperaba nombre de etiqueta").Value;
            Consume(TokenType.RightBracket, "Se esperaba ']' después de etiqueta");

            if (Match(TokenType.LeftParen))
            {
                bool condition = Condition();
                Consume(TokenType.RightParen, "Se esperaba ')' después de condición");

                if (!condition) return;
            }

            if (!_labels.TryGetValue(label, out int targetPos))
            {
                throw new Exception($"Etiqueta no definida: {label}");
            }

            throw new GotoException(targetPos);
        }

        private void IfStatement()
        {
            Consume(TokenType.LeftParen, "Se esperaba '(' después de If");
            bool condition = Condition();
            Consume(TokenType.RightParen, "Se esperaba ')' después de condición");

            if (!condition)
            {
                int depth = 1;
                while (!IsAtEnd() && depth > 0)
                {
                    if (Match(TokenType.If)) depth++;
                    else if (Match(TokenType.EndIf)) depth--;
                    else if (depth == 1 && Match(TokenType.Else)) break;
                    Advance();
                }
            }
        }

        private void WhileStatement()
        {
            int loopStart = _current;
            _loopStack.Push(loopStart);

            Consume(TokenType.LeftParen, "Se esperaba '(' después de While");
            bool condition = Condition();
            Consume(TokenType.RightParen, "Se esperaba ')' después de condición");
            Consume(TokenType.Do, "Se esperaba 'Do' después de condición While");

            if (!condition)
            {
                int depth = 1;
                while (!IsAtEnd() && depth > 0)
                {
                    if (Match(TokenType.While)) depth++;
                    else if (Match(TokenType.EndWhile)) depth--;
                    Advance();
                }
                _loopStack.Pop();
                return;
            }
        }

        // Asignación de variables con soporte para booleanos y llamadas a funciones
        private void Assignment()
        {
            string varName = Previous().Value;
            Consume(TokenType.Assign, "Se esperaba '<-' en asignación");

            if (Match(TokenType.True))
            {
                _boolVariables[varName] = true;
            }
            else if (Match(TokenType.False))
            {
                _boolVariables[varName] = false;
            }
            else if (IsFunctionCall())
            {
                _variables[varName] = FunctionCall();
            }
            else
            {
                _variables[varName] = Expression();
            }
        }

        // Verifica si la siguiente expresión es una llamada a función
        private bool IsFunctionCall()
        {
            if (Peek().Type != TokenType.Identifier) return false;

            // Mira el siguiente token después del identificador
            int nextPos = _current + 1;
            return nextPos < _tokens.Count && _tokens[nextPos].Type == TokenType.LeftParen;
        }

        // Ejecuta la llamada a función y devuelve el resultado
        private int FunctionCall()
        {
            string funcName = Consume(TokenType.Identifier, "Se esperaba nombre de función").Value.ToLower();
            Consume(TokenType.LeftParen, "Se esperaba '(' después de nombre de función");

            int result = 0;

            switch (funcName)
            {
                case "getactualx":
                    result = _form.GetActualX();
                    break;
                case "getactualy":
                    result = _form.GetActualY();
                    break;
                case "getcanvassize":
                    result = _form.GetCanvasSize();
                    break;
                case "isbrushcolor":
                    string color = Consume(TokenType.String, "Se esperaba color como string").Value.Trim('"');
                    result = _form.IsBrushColor(color);
                    break;
                case "isbrushsize":
                    int size = Expression();
                    result = _form.IsBrushSize(size);
                    break;
                case "iscanvascolor":
                    string canvasColor = Consume(TokenType.String, "Se esperaba color como string").Value.Trim('"');
                    Consume(TokenType.Comma, "Se esperaba ',' después de color");
                    int x = Expression();
                    Consume(TokenType.Comma, "Se esperaba ',' después de x");
                    int y = Expression();
                    result = _form.IsCanvasColor(canvasColor, x, y);
                    break;
                case "getcolorcount":
                    string countColor = Consume(TokenType.String, "Se esperaba color como string").Value.Trim('"');
                    Consume(TokenType.Comma, "Se esperaba ',' después de color");
                    int x1 = Expression();
                    Consume(TokenType.Comma, "Se esperaba ',' después de x1");
                    int y1 = Expression();
                    Consume(TokenType.Comma, "Se esperaba ',' después de y1");
                    int x2 = Expression();
                    Consume(TokenType.Comma, "Se esperaba ',' después de x2");
                    int y2 = Expression();
                    result = _form.GetColorCount(countColor, x1, y1, x2, y2);
                    break;
                default:
                    throw new Exception($"Función no reconocida: {funcName}");
            }

            Consume(TokenType.RightParen, "Se esperaba ')' al final de llamada a función");
            return result;
        }

        private void FillStatement()
        {
            Consume(TokenType.LeftParen, "Se esperaba '(' después de Fill");
            Consume(TokenType.RightParen, "Se esperaba ')' al final de Fill");
            _form.Fill();
        }

        // Evaluación de condiciones booleanas y comparaciones
        private bool Condition()
        {
            int leftValue = GetComparativeValue();

            if (Match(TokenType.Equal, TokenType.NotEqual, TokenType.LessThan,
                     TokenType.GreaterThan, TokenType.LessOrEqual, TokenType.GreaterOrEqual))
            {
                var op = Previous();
                int rightValue = GetComparativeValue();

                return op.Type switch
                {
                    TokenType.Equal => leftValue == rightValue,
                    TokenType.NotEqual => leftValue != rightValue,
                    TokenType.LessThan => leftValue < rightValue,
                    TokenType.GreaterThan => leftValue > rightValue,
                    TokenType.LessOrEqual => leftValue <= rightValue,
                    TokenType.GreaterOrEqual => leftValue >= rightValue,
                    _ => false
                };
            }

            return leftValue != 0; // Cualquier valor distinto de 0 es true
        }

        private int GetComparativeValue()
        {
            if (IsFunctionCall())
            {
                return FunctionCall();
            }
            return Expression();
        }

        // Evaluación de expresiones booleanas complejas con NOT y paréntesis
        private bool BooleanExpression()
        {
            if (Match(TokenType.Not))
            {
                return !BooleanExpression();
            }

            if (Match(TokenType.LeftParen))
            {
                bool value = Condition();
                Consume(TokenType.RightParen, "Se esperaba ')' después de expresión booleana");
                return value;
            }

            // Manejar valores booleanos literales
            if (Match(TokenType.True)) return true;
            if (Match(TokenType.False)) return false;

            // Manejar variables booleanas
            if (Match(TokenType.Identifier) && _boolVariables.TryGetValue(Previous().Value, out bool boolValue))
            {
                return boolValue;
            }

            // Comparación numérica (0 = false, cualquier otro valor = true)
            int left = Expression();

            if (Match(TokenType.Equal, TokenType.NotEqual, TokenType.LessThan,
                     TokenType.GreaterThan, TokenType.LessOrEqual, TokenType.GreaterOrEqual))
            {
                var op = Previous();
                int right = Expression();

                return op.Type switch
                {
                    TokenType.Equal => left == right,
                    TokenType.NotEqual => left != right,
                    TokenType.LessThan => left < right,
                    TokenType.GreaterThan => left > right,
                    TokenType.LessOrEqual => left <= right,
                    TokenType.GreaterOrEqual => left >= right,
                    _ => false
                };
            }

            return left != 0; 
        }

        // Evaluación de expresiones aritméticas: suma y resta
        private int Expression()
        {
            int value = Term();

            while (Match(TokenType.Plus, TokenType.Minus))
            {
                var op = Previous();
                int right = Term();
                value = op.Type == TokenType.Plus ? value + right : value - right;
            }

            return value;
        }

        // Evaluación de términos: multiplicación, división y módulo
        private int Term()
        {
            int value = Factor();

            while (Match(TokenType.Multiply, TokenType.Divide, TokenType.Modulo))
            {
                var op = Previous();
                int right = Factor();

                value = op.Type switch
                {
                    TokenType.Multiply => value * right,
                    TokenType.Divide => right != 0 ? value / right : throw new Exception("División por cero"),
                    TokenType.Modulo => right != 0 ? value % right : throw new Exception("Módulo por cero"),
                    _ => value
                };
            }

            return value;
        }

        // Evaluación de factores, incluyendo negación y potencia
        private int Factor()
        {
            if (Match(TokenType.Minus))
            {
                return -Factor();
            }

            if (Match(TokenType.Power))
            {
                int left = Primary();
                Consume(TokenType.Power, "Se esperaba '**' para operación de potencia");
                int right = Factor();
                return (int)Math.Pow(left, right);
            }

            return Primary();
        }

        // Evaluación de valores primarios: números, variables, expresiones entre paréntesis
        private int Primary()
        {
            if (Match(TokenType.Number))
            {
                return int.Parse(Previous().Value);
            }

            if (Match(TokenType.Identifier))
            {
                string varName = Previous().Value;
                if (_variables.TryGetValue(varName, out int value)) return value;
                if (IsFunctionCall()) return FunctionCall();
                throw new Exception($"Variable no definida: {varName}");
            }

            if (Match(TokenType.LeftParen))
            {
                int value = Expression();
                Consume(TokenType.RightParen, "Se esperaba ')' después de expresión");
                return value;
            }

            throw new Exception($"Se esperaba expresión, se encontró: {Peek().Value}");
        }

        // Métodos auxiliares para manejo de tokens

        private bool Match(params TokenType[] types)
        {
            if (!Check(types)) return false;
            Advance();
            return true;
        }

        private bool Check(params TokenType[] types)
        {
            if (IsAtEnd()) return false;
            return types.Contains(Peek().Type);
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw new Exception(message);
        }

        private bool IsAtEnd() => Peek().Type == TokenType.EOF;

        private Token Peek() => _tokens[_current];

        private Token Previous() => _tokens[_current - 1];
    }

    public class GotoException : Exception
    {
        public int TargetPosition { get; }
        public GotoException(int targetPosition) : base($"Saltando a posición {targetPosition}")
        {
            TargetPosition = targetPosition;
        }
    }
}