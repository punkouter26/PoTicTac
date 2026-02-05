import { Routes, Route } from 'react-router-dom';
import { HomePage } from './pages/HomePage';
import { LeaderboardPage } from './pages/LeaderboardPage';
import { Layout } from './components/Layout';

/**
 * Main App component with routing configuration.
 * Uses React Router for client-side navigation.
 */
function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/leaderboard" element={<LeaderboardPage />} />
      </Route>
    </Routes>
  );
}

export default App;
