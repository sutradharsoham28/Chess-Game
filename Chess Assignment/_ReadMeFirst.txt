Topic: 
Chess Possible Moves Calculator
You have to calculate all possible legal moves for a chess piece and highlight cells when a chess piece is selected.
No need to implement movement part, you only need to highlight the cells [Use Helper Functions provided inside Template project].
You can change initial positions of chess pieces as shown in the image for testing various cases.
Primary Goal: Coding style and structure of the scripts will be checked in the project.
Please don't waste time in UI/UX part which is not the primary goal.
Use OOPs, design patterns & good coding practices wherever possible.

Extension:
Add enemy pieces and highlight red if you can take them.

Helper Functions in the Template Project:

ChessBoardPlacementHandler.Instance.GetTile(row, column); // returns game object of given row and column.
ChessBoardPlacementHandler.Instance.Highlight(row, column); // Highlights the given cell.
ChessBoardPlacementHandler.Instance.ClearHighlights(); // Clears all previous highlights

To change the inital position of a piece just update the row and column value in inspector. See image for clarification

--------------------------------------------------------------------------
You have to send this project in a zip file containing 3 folders - 
1) Assets
2) Packages
3) Project Settings

Game orientation should be Landscape.
Please use Unity 2020.3.X version.