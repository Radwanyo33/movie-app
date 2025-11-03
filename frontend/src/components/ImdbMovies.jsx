import React from "react";
import "../index.css";
import "./ImdbMovies.css";
import { MoviesCards } from "./MoviesCards"

const ImdbMovies = ({ movies = [], isAdmin, onMovieEdit, onMovieDelete }) => {
  return (
    <ul className="movies-list grid grid-three--cols">
        {movies.map((movie) => (
          <MoviesCards 
            key={movie.id} 
            data={movie} 
            isAdmin={isAdmin}
            onEdit={onMovieEdit}
            onDelete={onMovieDelete}
          />
        ))}  
    </ul>
  );
}

export default ImdbMovies;