## Pixel Wall-E - Informe Técnico
## Descripción del Proyecto
Lenguaje de programación interpretado para creación de arte pixelado mediante comandos simples. Permite:
- Dibujo vectorial con comandos como `DrawLine` y `DrawCircle`
- Estructuras de control de flujo (`if`, `while`)
- Sistema de variables y funciones integradas

##Características clave:
- Canvas redimensionable
- Editor con resaltado de sintaxis
- Exportación de imágenes

## Requisitos Técnicos
Entorno de Desarrollo:
- Visual Studio 2022
- .NET Framework 4.7.2
- Windows 10/11

## Ejecución:
1. Clonar repositorio
2. Abrir PixelWallE.sln
3. Compilar en modo Release (Ctrl+Shift+B)
4. Ejecutar PixelWallE/bin/Release/PixelWallE.exe

## Tutorial de Uso
## Interfaz Principal:
- Editor de Código: Área para escribir scripts con numeración de líneas
- Canvas: Área de dibujo (tamaño configurable)
- Controles:
Ejecutar: Procesa el código
Cargar/Guardar: Maneja archivos .pw

## Arquitectura del Sistema:
graph TD
    A[Lexer] --> B[Parser]
    B --> C[Interprete]
    C --> D[Renderer]
    D --> E[Canvas]

## Componentes Principales
1. Lexer (Analizador Léxico)
- Convierte texto en tokens
- Identifica:
public enum TokenType {
    Spawn, Color, DrawLine, DrawCircle,
    If, While, Number, String
}

2. Parser (Analizador Sintáctico)
- Valida estructura del código
- Implementa gramática:
program → Spawn statement+
statement → Command | IfBlock | WhileBlock
Command → DrawCommand | ColorCommand

3. Motor de Renderizado
- Algoritmo Bresenham para líneas
- Flood Fill para rellenos
- Manejo de capas gráficas

## Manejo de Errores
Error/	Causa/	Solución
Spawn no encontrado/	Falta comando inicial/	Añadir Spawn(x,y) al inicio
Color inválido/	Nombre de color no existe/	Usar colores predefinidos
División por 0/	Operación matemática inválida/	Validar denominador

## Estrategias de Implementación
1. Backtracking para Canvas
- Matriz bidimensional para representar estado del canvas
- Sistema de capas para operaciones de dibujo

2. Sistema de Comandos
interface ICommand {
    void Execute();
}

class DrawLineCommand : ICommand {
    public void Execute() {
        // Implementación Bresenham
    }
}

3. Optimizaciones
- Doble buffer para renderizado
- Cache de operaciones frecuentes

## Roadmap
- Exportación a formatos PNG/SVG
- Historial de deshacer/rehacer
- Biblioteca de sprites integrada

