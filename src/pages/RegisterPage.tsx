import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import RegisterForm from '../components/RegisterForm';
import { useAuth } from '../contexts/AuthContext';

export default function RegisterPage() {
  const { register, user, loading, error } = useAuth();
  const navigate = useNavigate();

  // Redirect if already authenticated
  useEffect(() => {
    if (user) {
      navigate('/');
    }
  }, [user, navigate]);

  const handleRegister = async (username: string, email: string, firstName: string, lastName: string, password: string) => {
    await register(username, email, firstName, lastName, password);
    // Navigation will happen via useEffect when user is set
  };

  return (
    <RegisterForm
      onRegister={handleRegister}
      loading={loading}
      error={error}
    />
  );
}