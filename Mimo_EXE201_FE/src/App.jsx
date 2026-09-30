import React, { useState } from 'react';
import MimoDashboard from './components/MimoDashboard';
import MimoLandingPage from './components/MimoLandingPage';
import MimoRegisterPage from './components/MimoRegisterPage';
import './App.css'; 

function App() {
  const [currentView, setCurrentView] = useState('landing'); // 'landing', 'register', 'dashboard'

  return (
    <>
      {currentView === 'dashboard' && <MimoDashboard />}
      
      {currentView === 'register' && (
        <MimoRegisterPage 
          onRegisterSuccess={() => setCurrentView('dashboard')}
          onLoginClick={() => setCurrentView('dashboard')} // For now, login just goes to dashboard
        />
      )}
      
      {currentView === 'landing' && (
        <MimoLandingPage 
          onStart={() => setCurrentView('dashboard')} 
          onRegisterClick={() => setCurrentView('register')}
        />
      )}
    </>
  );
}

export default App;
