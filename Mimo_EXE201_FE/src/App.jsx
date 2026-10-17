import React, { useState } from 'react';
import MimoDashboard from './components/MimoDashboard';
import MimoLandingPage from './components/MimoLandingPage';
import MimoRegisterPage from './components/MimoRegisterPage';
import MimoLoginPage from './components/MimoLoginPage';
import './App.css'; 

function App() {
  const [currentView, setCurrentView] = useState('landing'); // 'landing', 'register', 'dashboard', 'login'

  return (
    <>
      {currentView === 'dashboard' && <MimoDashboard />}
      
      {currentView === 'register' && (
        <MimoRegisterPage 
          onRegisterSuccess={() => setCurrentView('dashboard')}
          onLoginClick={() => setCurrentView('login')}
          onHomeClick={() => setCurrentView('landing')}
        />
      )}

      {currentView === 'login' && (
        <MimoLoginPage 
          onLoginSuccess={() => setCurrentView('dashboard')}
          onRegisterClick={() => setCurrentView('register')}
          onHomeClick={() => setCurrentView('landing')}
        />
      )}
      
      {currentView === 'landing' && (
        <MimoLandingPage 
          onStart={() => setCurrentView('dashboard')} 
          onRegisterClick={() => setCurrentView('register')}
          onLoginClick={() => setCurrentView('login')}
        />
      )}
    </>
  );
}

export default App;
