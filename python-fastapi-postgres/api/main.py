from fastapi import FastAPI
from typing import List

from models import User, UserCreate
from database import DatabaseManager, UserRepository

app = FastAPI(title="User API", description="Simple CRUD API with PostgreSQL")

@app.get("/")
def read_root():
    return {"message": "User API", "endpoints": ["/users", "/users/{id}", "/health"]}


@app.get("/health")
async def health_check():
    return await DatabaseManager.check_health()


@app.on_event("startup")
async def startup_event():
    # Create users table if it doesn't exist
    await DatabaseManager.initialize_database()


@app.get("/users", response_model=List[User])
async def get_users():
    return await UserRepository.get_all()


@app.get("/users/{user_id}", response_model=User)
async def get_user(user_id: int):
    return await UserRepository.get_by_id(user_id)


@app.post("/users", response_model=User)
async def create_user(user: UserCreate):
    return await UserRepository.create(user)


@app.delete("/users/{user_id}")
async def delete_user(user_id: int):
    return await UserRepository.delete(user_id)
