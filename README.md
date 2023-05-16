# MovieLibraryOO
**An example of using OO principles to complete an in-class assignment for WCTC**

This project includes 
* the legacy FileService to read/write from files, and,
* the updated MovieLibraryEntities project which sets up the database.

Note the implementation is ***not complete*** for the MovieLibraryEntities Repository as it currently
does not meet all requirements for the full functionality.  
* Implemented
    * Get 
    * Search
* Not Implemented
    * Add
    * Update
    * Delete

An example test also exists for the Mapper.

---
## Additional Packages Used

This application uses a number of libraries (NuGet packages) for fun and ease of implementation. 
Below are the packages and the alternative methods you could use if you do not wish to use them.

### Spectre.Console 
#### https://spectreconsole.net
This package provides fun interactive menus and display options for data including navigable menus and output in Ascii tables.

An alternative to this package is to simply use the Console.Writeline and Console.Readline

### Automapper
#### https://automapper.org
AutoMapper is a simple little library built to solve a deceptively complex problem -
getting rid of code that mapped one object to another.

This was used in the project to prevent the user from seeing the full database models on output.
To accomplish this, a separate model was created (MovieDto) that contained a smaller set of properties.
To output the model, it was necessary to map the database model, Movie, to MovieDto.  Automapper simply automagically took care of this.

An alternative to Automapper is to either not worry about outputting the database model, or you can just map/assign the properties
of the different models yourself.
