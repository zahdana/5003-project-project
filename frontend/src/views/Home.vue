<template>
    <div class="home-container">
      <div class="welcome-message">
        <h1>welcome</h1>
        <p></p>
        <div class="action-buttons">
          <router-link to="/login" class="btn primary-btn">login</router-link>
          <router-link to="/register" class="btn secondary-btn">register</router-link>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    name: 'HomePage',
  };
  </script>
  
  <style scoped>
  .home-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
    background-color: #f7f7f7;
    font-family: 'Arial', sans-serif;
  }
  
  .welcome-message {
    background-color: #fff;
    padding: 2rem;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
    text-align: center;
    width: 100%;
    max-width: 600px;
  }
  
  h1 {
    font-size: 2.5rem;
    color: #333;
    margin-bottom: 1rem;
  }
  
  p {
    font-size: 1.1rem;
    color: #666;
    margin-bottom: 2rem;
  }
  
  .action-buttons {
    display: flex;
    justify-content: center;
    gap: 1rem;
  }
  
  .btn {
    padding: 0.8rem 2rem;
    border-radius: 4px;
    font-size: 1.1rem;
    text-decoration: none;
    cursor: pointer;
    transition: all 0.3s;
  }
  
  .primary-btn {
    background-color: #4CAF50;
    color: white;
  }
  
  .primary-btn:hover {
    background-color: #45a049;
  }
  
  .secondary-btn {
    background-color: #007bff;
    color: white;
  }
  
  .secondary-btn:hover {
    background-color: #0056b3;
  }
  </style>
  