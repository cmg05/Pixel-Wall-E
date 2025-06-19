using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel_Wall_E
{
    public class WallEInterpreter
    {
        public class GotoException : Exception
        {
            public int TargetLine { get; }

            public GotoException(int targetLine) : base("Salto condicional ejecutado")
            {
                TargetLine = targetLine;
            }
        }

        private readonly Form1 _form;
        private readonly Dictionary<string, int> _variables = new Dictionary<string, int>();
        private readonly Dictionary<string, bool> _boolVariables = new Dictionary<string, bool>();
        private readonly Dictionary<string, int> _labels = new Dictionary<string, int>();
        private readonly Dictionary<string, Func<int>> _builtInFunctions = new Dictionary<string, Func<int>>();

        // Variables para almacenar parámetros de funciones
        private string _lastColorChecked;
        private int _lastSizeChecked;
        private int _lastVertical;
        private int _lastHorizontal;
        private bool _hasSpawned = false;

        public WallEInterpreter(Form1 form)
        {
            _form = form;
            InitializeBuiltInFunctions();
        }

        private void InitializeBuiltInFunctions()
        {
            _builtInFunctions["getactualx"] = _form.GetActualX;
            _builtInFunctions["getactualy"] = _form.GetActualY;
            _builtInFunctions["getcanvassize"] = _form.GetCanvasSize;
            _builtInFunctions["isbrushcolor"] = () => _form.IsBrushColor(_lastColorChecked);
            _builtInFunctions["isbrushsize"] = () => _form.IsBrushSize(_lastSizeChecked);
            _builtInFunctions["iscanvascolor"] = () => _form.IsCanvasColor(_lastColorChecked, _lastVertical, _lastHorizontal);
        }

        public void ProcessLabels(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Ignorar líneas vacías o comentarios
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                // Detectar etiquetas (terminan con :)
                if (line.EndsWith(":") && !line.Contains(" "))
                {
                    string label = line.Substring(0, line.Length - 1).ToLower();

                    if (_labels.ContainsKey(label))
                        throw new Exception($"Etiqueta duplicada: '{label}'");

                    _labels[label] = i; // Guardar el número de línea (0-based)
                }
            }
        }

        public void ExecuteCommand(string command, int lineNumber)
        {
            try
            {
                command = command.Trim();

                // Ignorar líneas vacías o comentarios
                if (string.IsNullOrWhiteSpace(command) || command.StartsWith("//") || command.EndsWith(":"))
                {
                    return;
                }

                if (command.EndsWith(":"))
                {
                    return;
                }
                else if (command.StartsWith("spawn(", StringComparison.OrdinalIgnoreCase))
                {
                    if (_hasSpawned)
                        throw new Exception("Spawn solo puede llamarse una vez al inicio del programa");

                    ExecuteSpawn(command);
                    _hasSpawned = true;
                }
                else if (!_hasSpawned)
                {
                    throw new Exception("El programa debe comenzar con un comando Spawn");
                }
                else if (command.StartsWith("color(", StringComparison.OrdinalIgnoreCase))
                {
                    ExecuteColor(command);
                }
                else if (command.StartsWith("size(", StringComparison.OrdinalIgnoreCase))
                {
                    ExecuteSize(command);
                }
                else if (command.StartsWith("drawline(", StringComparison.OrdinalIgnoreCase))
                {
                    ExecuteDrawLine(command);
                }
                else if (command.StartsWith("drawcircle(", StringComparison.OrdinalIgnoreCase))
                {
                    ExecuteDrawCircle(command);
                }
                else if (command.StartsWith("drawrectangle(", StringComparison.OrdinalIgnoreCase))
                {
                    ExecuteDrawRectangle(command);
                }
                else if (command.Equals("fill()", StringComparison.OrdinalIgnoreCase))
                {
                    _form.Fill();
                }
                else if (command.StartsWith("goto[", StringComparison.OrdinalIgnoreCase))
                {
                    ExecuteGoto(command, lineNumber);
                }
                else if (command.Contains("<-"))
                {
                    ExecuteVariableAssignment(command);
                }
                else if (command.StartsWith("getcolorcount(", StringComparison.OrdinalIgnoreCase))
                {
                    ExecuteGetColorCount(command);
                }
                else if (command.StartsWith("isbrushcolor(", StringComparison.OrdinalIgnoreCase) ||
                         command.StartsWith("isbrushsize(", StringComparison.OrdinalIgnoreCase) ||
                         command.StartsWith("iscanvascolor(", StringComparison.OrdinalIgnoreCase))
                {
                    ExecuteFunctionWithParams(command);
                }
                else
                {
                    throw new Exception($"Comando no reconocido: '{command}'");
                }
            }
            catch (GotoException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en línea {lineNumber}: {ex.Message}");
            }
        }

        private void ExecuteSpawn(string command)
        {
            var parameters = ExtractParameters(command, 2);
            int x = EvaluateExpression(parameters[0]);
            int y = EvaluateExpression(parameters[1]);
            _form.ExecuteSpawn(x, y);
        }

        private void ExecuteColor(string command)
        {
            string color = command.Split('(', ')')[1].Replace("\"", "").Trim();
            _form.SetColor(color);
        }

        private void ExecuteSize(string command)
        {
            int size = EvaluateExpression(ExtractParameters(command, 1)[0]);
            _form.SetBrushSize(size);
        }

        private void ExecuteDrawLine(string command)
        {
            var parameters = ExtractParameters(command, 3);
            _form.DrawLine(
                EvaluateExpression(parameters[0]),
                EvaluateExpression(parameters[1]),
                EvaluateExpression(parameters[2])
            );
        }

        private void ExecuteDrawCircle(string command)
        {
            var parameters = ExtractParameters(command, 3);
            _form.DrawCircle(
                EvaluateExpression(parameters[0]),
                EvaluateExpression(parameters[1]),
                EvaluateExpression(parameters[2])
            );
        }

        private void ExecuteDrawRectangle(string command)
        {
            var parameters = ExtractParameters(command, 5);
            _form.DrawRectangle(
                EvaluateExpression(parameters[0]),
                EvaluateExpression(parameters[1]),
                EvaluateExpression(parameters[2]),
                EvaluateExpression(parameters[3]),
                EvaluateExpression(parameters[4])
            );
        }

        private void ExecuteGoto(string command, int currentLine)
        {
            int labelStart = command.IndexOf('[') + 1;
            int labelEnd = command.IndexOf(']');
            if (labelStart < 1 || labelEnd < labelStart)
                throw new Exception("Formato Goto incorrecto. Use: Goto[etiqueta](condición)");

            string label = command.Substring(labelStart, labelEnd - labelStart).Trim().ToLower();

            string condition = "true";
            int condStart = command.IndexOf('(');
            if (condStart > labelEnd)
            {
                int condEnd = command.IndexOf(')', condStart);
                if (condEnd < 0) condEnd = command.Length;
                condition = command.Substring(condStart + 1, condEnd - condStart - 1).Trim();
            }

            if (!_labels.ContainsKey(label))
                throw new Exception($"Etiqueta no encontrada: '{label}'");

            if (EvaluateCondition(condition))
                throw new GotoException(_labels[label]);
        }

        private void ExecuteVariableAssignment(string command)
        {
            var parts = command.Split(new[] { "<-" }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new Exception("Asignación no válida. Formato: variable <- expresión");

            string varName = parts[0].Trim().ToLower();
            string expression = parts[1].Trim();

            if (!IsValidVariableName(varName))
                throw new Exception($"Nombre de variable no válido: '{varName}'");

            try
            {
                // Evaluar la expresión del lado derecho (puede ser función, variable o expresión)
                int value = EvaluateFunctionOrExpression(expression);

                // Asignar el valor a la variable
                _variables[varName] = value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al evaluar expresión para asignación a '{varName}': {ex.Message}");
            }
        }

        private int EvaluateFunctionOrExpression(string input)
        {
            input = input.Trim().ToLower();

            // 1. Verificar si es una función incorporada
            if (input.EndsWith(")"))
            {
                string funcName = input.Split('(')[0];
                if (_builtInFunctions.ContainsKey(funcName))
                {
                    // Manejar funciones sin parámetros
                    if (input.Contains("()"))
                    {
                        return _builtInFunctions[funcName]();
                    }
                    // Manejar funciones con parámetros
                    else
                    {
                        ExecuteFunctionWithParams(input);
                        return _builtInFunctions[funcName]();
                    }
                }
            }

            // 2. Si no es función, evaluar como expresión normal
            return EvaluateExpression(input);
        }


        private void ExecuteGetColorCount(string command)
        {
            var parameters = ExtractParameters(command, 5);
            string color = parameters[0].Replace("\"", "").Trim();
            int x1 = EvaluateExpression(parameters[1]);
            int y1 = EvaluateExpression(parameters[2]);
            int x2 = EvaluateExpression(parameters[3]);
            int y2 = EvaluateExpression(parameters[4]);

            int count = _form.GetColorCount(color, x1, y1, x2, y2);
        }

        private void ExecuteFunctionWithParams(string command)
        {
            var parts = command.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string funcName = parts[0].Trim().ToLower();
            string[] parameters = parts[1].Split(',').Select(p => p.Trim()).ToArray();

            if (funcName == "isbrushcolor" && parameters.Length == 1)
            {
                _lastColorChecked = parameters[0].Replace("\"", "");
            }
            else if (funcName == "isbrushsize" && parameters.Length == 1)
            {
                _lastSizeChecked = EvaluateExpression(parameters[0]);
            }
            else if (funcName == "iscanvascolor" && parameters.Length == 3)
            {
                _lastColorChecked = parameters[0].Replace("\"", "");
                _lastVertical = EvaluateExpression(parameters[1]);
                _lastHorizontal = EvaluateExpression(parameters[2]);
            }
            else
            {
                throw new Exception($"Parámetros incorrectos para {funcName}");
            }
        }

        private string[] ExtractParameters(string command, int expectedCount)
        {
            int start = command.IndexOf('(') + 1;
            int end = command.LastIndexOf(')');
            string paramStr = command.Substring(start, end - start);
            string[] parameters = paramStr.Split(',').Select(p => p.Trim()).ToArray();

            if (parameters.Length != expectedCount)
            {
                throw new Exception($"Número incorrecto de parámetros. Esperados: {expectedCount}, Recibidos: {parameters.Length}");
            }

            return parameters;
        }

        private int EvaluateExpression(string expression)
        {
            expression = expression.Trim().ToLower();

            // Verificar si es un número directo
            if (int.TryParse(expression, out int numericValue))
            {
                return numericValue;
            }

            // Verificar si es una variable existente
            if (_variables.TryGetValue(expression, out int varValue))
            {
                return varValue;
            }

            // Verificar si es una expresión matemática con variables
            if (ContainsMathOperators(expression))
            {
                return EvaluateMathExpressionWithVariables(expression);
            }

            throw new Exception($"No se puede evaluar la expresión: '{expression}'");
        }

        private int EvaluateMathExpressionWithVariables(string expression)
        {
            // Primero reemplazar todas las variables por sus valores
            foreach (var variable in _variables)
            {
                expression = expression.Replace(variable.Key, variable.Value.ToString());
            }

            // Luego evaluar la expresión matemática
            return EvaluatePureMathExpression(expression);
        }

        private int EvaluatePureMathExpression(string expression)
        {
            try
            {
                expression = expression.Replace(" ", "");

                // Manejar paréntesis
                while (expression.Contains('('))
                {
                    int open = expression.LastIndexOf('(');
                    int close = expression.IndexOf(')', open);

                    if (close == -1) throw new Exception("Paréntesis no balanceados");

                    string subExpr = expression.Substring(open + 1, close - open - 1);
                    int subResult = EvaluatePureMathExpression(subExpr);
                    expression = expression.Remove(open, close - open + 1).Insert(open, subResult.ToString());
                }

                // Evaluar operadores en orden de precedencia
                expression = EvaluateMathOperator(expression, "**"); // Potencia
                expression = EvaluateMathOperator(expression, "*", "/", "%"); // Mult/Div/Mod
                expression = EvaluateMathOperator(expression, "+", "-"); // Suma/Resta

                return int.Parse(expression);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al evaluar expresión matemática: '{expression}'. {ex.Message}");
            }
        }

        private string EvaluateMathOperator(string expr, params string[] operators)
        {
            for (int i = 0; i < expr.Length; i++)
            {
                foreach (string op in operators)
                {
                    if (i + op.Length <= expr.Length && expr.Substring(i, op.Length) == op)
                    {
                        // Manejar signo negativo
                        if (op == "-" && (i == 0 || "+-*/%(".Contains(expr[i - 1].ToString())))
                        {
                            continue;
                        }

                        int leftStart = FindOperandStart(expr, i - 1);
                        int rightEnd = FindOperandEnd(expr, i + op.Length);

                        string leftStr = expr.Substring(leftStart, i - leftStart);
                        string rightStr = expr.Substring(i + op.Length, rightEnd - (i + op.Length));

                        int left = int.Parse(leftStr);
                        int right = int.Parse(rightStr);

                        int result = op switch
                        {
                            "**" => (int)Math.Pow(left, right),
                            "*" => left * right,
                            "/" => right == 0 ? throw new Exception("División por cero") : left / right,
                            "%" => left % right,
                            "+" => left + right,
                            "-" => left - right,
                            _ => throw new Exception($"Operador no soportado: '{op}'")
                        };

                        expr = expr[..leftStart] + result.ToString() + expr[rightEnd..];
                        i = leftStart;
                        break;
                    }
                }
            }
            return expr;
        }


        private bool ContainsMathOperators(string expr)
        {
            return expr.Contains('+') || expr.Contains('-') ||
                   expr.Contains('*') || expr.Contains('/') ||
                   expr.Contains('%') || expr.Contains("**");
        }

        private int EvaluateMathExpression(string expression)
        {
            try
            {
                // Eliminar espacios
                expression = expression.Replace(" ", "");

                // Manejar paréntesis primero
                while (expression.Contains('('))
                {
                    int openParen = expression.LastIndexOf('(');
                    int closeParen = expression.IndexOf(')', openParen);

                    if (closeParen == -1)
                        throw new Exception("Paréntesis no balanceados");

                    string subExpr = expression.Substring(openParen + 1, closeParen - openParen - 1);
                    int subResult = EvaluateMathExpression(subExpr);
                    expression = expression.Remove(openParen, closeParen - openParen + 1)
                                     .Insert(openParen, subResult.ToString());
                }

                // Evaluar operadores en orden de precedencia
                expression = EvaluateOperator(expression, "**");  // Potencia
                expression = EvaluateOperator(expression, "*", "/", "%");  // Multiplicación/división/módulo
                expression = EvaluateOperator(expression, "+", "-");  // Suma/resta

                // Verificar si el resultado es un número válido
                if (int.TryParse(expression, out int result))
                {
                    return result;
                }
                throw new Exception($"Expresión no pudo ser evaluada completamente: '{expression}'");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al evaluar expresión matemática: '{expression}'. {ex.Message}");
            }
        }

        private string EvaluateOperator(string expression, params string[] operators)
        {
            int i = 0;
            while (i < expression.Length)
            {
                foreach (string op in operators)
                {
                    if (i + op.Length <= expression.Length &&
                        expression.Substring(i, op.Length) == op)
                    {
                        // Manejar operadores unarios (como negativo)
                        if (op == "-" && (i == 0 || "+-*/%(".Contains(expression[i - 1].ToString())))
                        {
                            i++;
                            continue;
                        }

                        // Encontrar operandos
                        int leftStart = FindOperandStart(expression, i - 1);
                        int rightEnd = FindOperandEnd(expression, i + op.Length);

                        string leftStr = expression.Substring(leftStart, i - leftStart);
                        string rightStr = expression.Substring(i + op.Length, rightEnd - (i + op.Length));

                        int left = EvaluateExpression(leftStr);
                        int right = EvaluateExpression(rightStr);

                        int result = op switch
                        {
                            "**" => (int)Math.Pow(left, right),
                            "*" => left * right,
                            "/" => right == 0 ? throw new Exception("División por cero") : left / right,
                            "%" => left % right,
                            "+" => left + right,
                            "-" => left - right,
                            _ => throw new Exception($"Operador no soportado: '{op}'")
                        };

                        // Reemplazar la expresión con el resultado
                        expression = expression[..leftStart] + result.ToString() + expression[rightEnd..];
                        i = leftStart;
                        break;
                    }
                }
                i++;
            }
            return expression;
        }

        private int FindOperandStart(string expression, int position)
        {
            int start = position;
            while (start >= 0)
            {
                char c = expression[start];
                // Permitir dígitos, punto decimal y signo negativo al inicio
                if (char.IsDigit(c) || c == '.' || (c == '-' && (start == 0 || "+-*/%(".Contains(expression[start - 1].ToString()))))
                {
                    start--;
                }
                else
                {
                    break;
                }
            }
            return start + 1; // Ajustar a la posición correcta
        }

        private int FindOperandEnd(string expression, int position)
        {
            int end = position;
            while (end < expression.Length)
            {
                char c = expression[end];
                // Permitir dígitos y punto decimal
                if (char.IsDigit(c) || c == '.')
                {
                    end++;
                }
                else
                {
                    break;
                }
            }
            return end;
        }

        private int FindOperandStart(string expression, int start, bool allowNegative)
        {
            while (start >= 0 && (char.IsDigit(expression[start]) ||
                  (allowNegative && expression[start] == '-' && (start == 0 || !char.IsDigit(expression[start - 1])))))
            {
                start--;
            }
            return start + 1;
        }

        private int FindOperandEnd(string expression, int start, bool allowNegative)
        {
            int end = start;
            if (allowNegative && end < expression.Length && expression[end] == '-')
            {
                end++;
            }
            while (end < expression.Length && char.IsDigit(expression[end]))
            {
                end++;
            }
            return end;
        }

        private bool EvaluateCondition(string condition)
        {
            condition = condition.Trim().ToLower();

            if (condition == "true") return true;
            if (condition == "false") return false;

            if (_boolVariables.TryGetValue(condition, out bool boolVar))
            {
                return boolVar;
            }

            string[] comparisonOps = { "==", "!=", ">=", "<=", ">", "<" };
            foreach (var op in comparisonOps)
            {
                if (condition.Contains(op))
                {
                    var parts = condition.Split(new[] { op }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2) continue;

                    int left = EvaluateExpression(parts[0].Trim());
                    int right = EvaluateExpression(parts[1].Trim());

                    return op switch
                    {
                        "==" => left == right,
                        "!=" => left != right,
                        ">=" => left >= right,
                        "<=" => left <= right,
                        ">" => left > right,
                        "<" => left < right,
                        _ => false
                    };
                }
            }

            if (condition.Contains("&&") || condition.Contains("||"))
            {
                string[] logicalOps = condition.Contains("&&") ? new[] { "&&" } : new[] { "||" };

                var parts = condition.Split(logicalOps, StringSplitOptions.RemoveEmptyEntries);
                bool result = EvaluateCondition(parts[0].Trim());

                for (int i = 1; i < parts.Length; i++)
                {
                    bool next = EvaluateCondition(parts[i].Trim());
                    result = logicalOps[0] == "&&" ? result && next : result || next;
                }

                return result;
            }

            try
            {
                int numValue = EvaluateExpression(condition);
                return numValue != 0;
            }
            catch
            {
                throw new Exception($"No se puede evaluar la condición: '{condition}'");
            }
        }

        private bool IsBooleanExpression(string expression)
        {
            expression = expression.Trim().ToLower();
            return expression.Contains("&&") || expression.Contains("||") ||
                   expression.Contains("==") || expression.Contains("!=") ||
                   expression.Contains(">=") || expression.Contains("<=") ||
                   expression.Contains(">") || expression.Contains("<") ||
                   expression == "true" || expression == "false" ||
                   _boolVariables.ContainsKey(expression);
        }

        private bool IsValidVariableName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (char.IsDigit(name[0]))
                return false;

            return name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }
    }
}